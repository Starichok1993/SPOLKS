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

            Console.WriteLine("Computing time: {0} ms", (stop - start).Milliseconds);
        }

        private void GatherResults(int numberOfSlaves)
        {
            var com = Communicator.world;
            int messageType = FROM_WORKER;

            for (int source = 1; source <= numberOfSlaves; source++)
            {
                var rowOffset = com.ImmediateReceive<int>(source, messageType);
                rowOffset.Wait();
                var rows = com.ImmediateReceive<int>(source, messageType);
                rows.Wait();
                var temp = com.ImmediateReceive<double[][]>(source, messageType);
                temp.Wait();
                Console.Write("Recive from {0}-worker {1} rows. Time:{2}\n", source, (int)rows.GetValue(), DateTime.Now.ToString("hh:mm:ss.ffff"));
                for (var j = 0; j < (int)rows.GetValue(); j++)
                {
                    matrixC[(int)rowOffset.GetValue() + j] = ((double[][])temp.GetValue())[j];
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
                
                var req = com.ImmediateSend(offsetRow, destination, messageType);
                req.Wait();
                req = com.ImmediateSend(rows, destination, messageType);
                req.Wait();

                double[][] temp = new double[rows][];
                for (int i = 0; i < rows; i++)
                {
                    temp[i] = matrixA[i];
                }

                req = com.ImmediateSend(temp, destination, messageType);
                req.Wait();
                req = com.ImmediateSend(matrixB, destination, messageType);
                req.Wait();

                offsetRow += rows;
            }
        }

        private void StartSlave()
        {
            var com = Communicator.world;
            int messageType = FROM_MASTER;
            int source = 0;

            var offsetRow = com.ImmediateReceive<int>(source, messageType);
            offsetRow.Wait();
            var rows = com.ImmediateReceive<int>(source, messageType);
            rows.Wait();
            var mA = com.ImmediateReceive<double[][]>(source, messageType);
            mA.Wait();
            var mB = com.ImmediateReceive<double[][]>(source, messageType);
            mB.Wait();
            var mC = MatrixService.MultiplyMatrix((double[][])mA.GetValue(), (double[][])mB.GetValue());

            var req = com.ImmediateSend((int)offsetRow.GetValue(), source, FROM_WORKER);
            req.Wait();
            req = com.ImmediateSend((int)rows.GetValue(), source, FROM_WORKER);
            req.Wait();
            com.ImmediateSend(mC, source, FROM_WORKER);
        }

    }
}
