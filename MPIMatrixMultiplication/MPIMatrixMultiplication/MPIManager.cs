using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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

        public void Start(ref string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                int currentTaskId = Communicator.world.Rank;

                if (currentTaskId == 0)
                {
                    Init();
                    StartMaster();
                }
                if (currentTaskId > 0)
                {
                    StartSlave();
                }
            }
            Console.ReadLine();
        }

        private void StartMaster()
        {
            var com = Communicator.world;
            int numberOfSlaves = com.Size - 1;

            Console.WriteLine("Number of worker tasks = " + numberOfSlaves);
            Console.WriteLine("Matrix A:");
            MatrixService.PrintMatrix(matrixA);
            Console.WriteLine("Matrix B:");
            MatrixService.PrintMatrix(matrixB);

            DateTime start = DateTime.Now;

            ShareTheTask(numberOfSlaves);
            GatherResults(numberOfSlaves);

            DateTime stop = DateTime.Now;

            Console.WriteLine("Result matrix:");
            MatrixService.PrintMatrix(matrixC);

            Console.WriteLine("Computing time: " + (stop - start));
        }

        private void GatherResults(int numberOfSlaves)
        {
            var com = Communicator.world;
            int messageType = FROM_WORKER;

            for (int source = 1; source <= numberOfSlaves; source++)
            {
                int rowOffset = com.Receive<int>(source, messageType);
                int rows = com.Receive<int>(source, messageType);
                var temp = com.Receive<double[][]>(source, messageType);
                Console.Write("Recive from {0}-worker {1} rows. Time:{2}\n", source, rows, DateTime.Now.ToString("hh:mm:ss.ffff"));
                for (var j = 0; j < rows; j++)
                {
                    matrixC[rowOffset + j] = temp[j];
                }
            }
        }

        private void ShareTheTask(int numberOfSlaves)
        {
            var com = Communicator.world;
            int rowsPerWorker = matrixA.Length / numberOfSlaves;
            int remainingRows = matrixA.Length % numberOfSlaves;
            int offsetRow = 0;
            int messageType = FROM_MASTER;

            for (int destination = 1; destination <= numberOfSlaves; destination++)
            {
                int rows = (destination <= remainingRows) ? rowsPerWorker + 1 : rowsPerWorker;
                Console.WriteLine("Sending {0} rows from offset {1} to task {2}", rows, offsetRow, destination);
                
                com.Send(offsetRow, destination, messageType);
                com.Send(rows, destination, messageType);

                double[][] temp = new double[rows][];
                for (int i = 0; i < rows; i++)
                {
                    temp[i] = matrixA[i];
                }

                com.Send(temp, destination, messageType);
                com.Send(matrixB, destination, messageType);

                offsetRow += rows;
            }
        }

        private void StartSlave()
        {
            var com = Communicator.world;
            int messageType = FROM_MASTER;
            int sourse = 0;

            int offsetRow = com.Receive<int>(sourse, messageType);
            int rows = com.Receive<int>(sourse, messageType);
            var  mA = com.Receive<double[][]>(sourse, messageType);
            var mB = com.Receive<double[][]>(sourse, messageType);
            var mC = MatrixService.MultiplyMatrix(mA, mB);

            com.Send(offsetRow, sourse, FROM_WORKER);
            com.Send(rows, sourse, FROM_WORKER);
            com.Send(mC, sourse, FROM_WORKER);
        }

    }
}
