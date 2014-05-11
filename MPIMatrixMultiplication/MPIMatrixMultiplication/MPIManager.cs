using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace MPIMatrixMultiplication
{
    
    public class MPIManager
    {
        const int FROM_MASTER = 1;
        const int FROM_WORKER = 2;

        private readonly int _rowsA;
        private readonly int _columnsA;
        private readonly int _columnsB;

        private double[][] matrixA;
        private double[][] matrixB;
        private double[][] matrixC;

        public MPIManager(int rowsA, int columnsA, int rowsB)
        {
            _rowsA = rowsA;
            _columnsA = columnsA;
            _columnsB = rowsB;
        }

        private void Init()
        {
            MatrixService.CreateMatrix(out matrixA, _rowsA, _columnsA);
            MatrixService.RandomInitialization(matrixA);
            
            MatrixService.CreateMatrix(out matrixB, _columnsA, _columnsB);
            MatrixService.RandomInitialization(matrixB);
            
            MatrixService.CreateMatrix(out matrixC, _rowsA, _columnsB);
        }

        private void CreateFile(string fileName)
        {
            var rowsCount = matrixC.Length;
            var columnsCount = matrixC[0].Length;
            var file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            if (file.Length == 0)
            {
                file.Write(BitConverter.GetBytes(rowsCount), 0, sizeof(int));
                file.Write(BitConverter.GetBytes(columnsCount), 0, sizeof(int));

                var fileSize = sizeof(double) * (rowsCount * columnsCount);
                file.Write(new byte[fileSize], 0, fileSize);

                file.Close();
            }
        }

        public void Start(ref string[] args)
        {
            int groupCount = int.Parse(args[0]);

            using (new MPI.Environment(ref args))
            {
                int processsorCount = Communicator.world.Size;

                if (processsorCount < groupCount * 2)
                {
                    Console.WriteLine("Exception: for one group should be min 2 processor!");
                    return; 
                }

                var random = new Random();
                int remainingProcessor = processsorCount;
                var listRanks = new List<int[]>();
                var groupList = new List<Communicator>();

                var rank = 0;
                for (int i = 0; i < groupCount; i++)
                {
                    var newGroupSize = 2;
                    if (i == groupCount - 1)
                    {
                        newGroupSize = remainingProcessor;
                    }
                    else
                    {
                        if (Communicator.world.Rank == 0)
                        {
                            newGroupSize = random.Next(2, remainingProcessor - (groupCount - i - 1) * 2);
                        }

                        Communicator.world.Broadcast(ref newGroupSize, 0);
                    }

                    remainingProcessor -= newGroupSize;

                    var ranks = new int[newGroupSize];
                    for (int j = 0; j < newGroupSize; j++)
                    {
                        ranks[j] = rank++;
                    }
                    listRanks.Add(ranks);
                    var newGroup = Communicator.world.Group.IncludeOnly(ranks);
                    var communicator = Communicator.world.Create(newGroup);
                    groupList.Add(communicator);
                }


                for (int i = 0; i < groupCount; i++)
                {
                    if (listRanks[i].Contains(Communicator.world.Rank))
                    {
                        StartGroup(groupList[i], i);
                    }
                }
            }
        }

        private void StartGroup(Communicator communicator, int identity)
        {
                int currentTaskId = communicator.Rank;

                if (currentTaskId == 0)
                {
                    Init();
                    StartMaster(communicator, identity);
                }
                if (currentTaskId > 0)
                {
                    StartSlave(communicator, identity);
                }

        }

        private void StartMaster(Communicator communicator, int identity)
        {
            int numberOfSlaves = communicator.Size - 1;

            Console.WriteLine("Group:{0} Number of worker tasks = {1}", identity, numberOfSlaves);

            CreateFile("saveFileGroup" + identity);

            //Console.WriteLine("Matrix A:");
            //MatrixService.PrintMatrix(matrixA);
            //Console.WriteLine("Matrix B:");
            //MatrixService.PrintMatrix(matrixB);

            matrixA = MatrixService.ReadMatrix("inputFileMatrixA");
            matrixB = MatrixService.ReadMatrix("inputFileMatrixB");

            DateTime start = DateTime.Now;

            ShareTheTask(communicator, numberOfSlaves);
            GatherResults(communicator, numberOfSlaves);

            DateTime stop = DateTime.Now;

            Console.WriteLine("Result matrix:");
            MatrixService.PrintMatrix(matrixC);

            Console.WriteLine("Group: {0} Computing time: {1} ms", identity, (stop - start).Milliseconds);
        }

        private void GatherResults(Communicator communicator, int numberOfSlaves)
        {
            int messageType = FROM_WORKER;

            for (int source = 1; source <= numberOfSlaves; source++)
            {
                int rowOffset = communicator.Receive<int>(source, messageType);
                int rows = communicator.Receive<int>(source, messageType);
                var temp = communicator.Receive<double[][]>(source, messageType);
                for (var j = 0; j < rows; j++)
                {
                    matrixC[rowOffset + j] = temp[j];
                }
            }
        }

        private void ShareTheTask(Communicator communicator, int numberOfSlaves)
        {
            int rowsPerWorker = matrixA.Length / numberOfSlaves;
            int remainingRows = matrixA.Length % numberOfSlaves;
            int offsetRow = 0;
            int messageType = FROM_MASTER;

            ((Intracommunicator)communicator).Broadcast<double[][]>(ref matrixB, 0);

            for (int destination = 1; destination <= numberOfSlaves; destination++)
            {
                int rows = (destination <= remainingRows) ? rowsPerWorker + 1 : rowsPerWorker;
                //Console.WriteLine("Sending {0} rows from offset {1} to task {2}", rows, offsetRow, destination);
                
                communicator.Send(offsetRow, destination, messageType);
                communicator.Send(rows, destination, messageType);

                double[][] temp = new double[rows][];
                for (int i = 0; i < rows; i++)
                {
                    temp[i] = matrixA[i];
                }

                communicator.Send(temp, destination, messageType);
                //communicator.Send(matrixB, destination, messageType);

                offsetRow += rows;
            }
        }

        private void StartSlave(Communicator communicator, int identity)
        {
            int messageType = FROM_MASTER;
            int sourse = 0;

            double[][] mB = null;

            ((Intracommunicator)communicator).Broadcast<double[][]>(ref mB, 0);

            int offsetRow = communicator.Receive<int>(sourse, messageType);
            int rows = communicator.Receive<int>(sourse, messageType);
            var  mA = communicator.Receive<double[][]>(sourse, messageType);
            //var mB = communicator.Receive<double[][]>(sourse, messageType);
            var mC = MatrixService.MultiplyMatrix(mA, mB);

            MatrixService.WriteMatrix("saveFileGroup" + identity, mC, offsetRow);

            communicator.Send(offsetRow, sourse, FROM_WORKER);
            communicator.Send(rows, sourse, FROM_WORKER);
            communicator.Send(mC, sourse, FROM_WORKER);
        }

    }
}
