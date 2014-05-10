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
        private const int FROM_MASTER = 1;
        private const int FROM_WORKER = 2;
        private const int FROM_MASTER_OFFSET_ROWS = 3;
        private const int FROM_MASTER_ROWS = 4;
        private const int FROM_MASTER_DATA = 5;
        private const int FROM_MASTER_MATRIX_B = 6;
        private const int FROM_WORKER_OFFSET_ROWS = 7;
        private const int FROM_WORKER_ROWS = 8;
        private const int FROM_WORKER_RESULT = 9;

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
            //int[] ranks = {1, 2};
            //var newGroup = com.Group.IncludeOnly(ranks);

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
            //int messageType = FROM_WORKER;

            for (int source = 1; source <= numberOfSlaves; source++)
            {
                var rowOffset = com.ImmediateReceive<int>(source, FROM_WORKER_OFFSET_ROWS);
                var rows = com.ImmediateReceive<int>(source, FROM_WORKER_ROWS);
                var temp = com.ImmediateReceive<double[][]>(source, FROM_WORKER_RESULT);
                
                rowOffset.Wait();
                rows.Wait();
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

            for (int destination = 1; destination <= numberOfSlaves; destination++)
            {
                int rows = (destination <= remainingRows) ? rowsPerWorker + 1 : rowsPerWorker;
                Console.WriteLine("Sending {0} rows from offset {1} to task {2}", rows, offsetRow, destination);
                
                var reqOffsetRow = com.ImmediateSend(offsetRow, destination, FROM_MASTER_OFFSET_ROWS);
                var reqRows = com.ImmediateSend(rows, destination, FROM_MASTER_ROWS);
                
                double[][] temp = new double[rows][];
                for (int i = 0; i < rows; i++)
                {
                    temp[i] = matrixA[i];
                }

                var reqData = com.ImmediateSend(temp, destination, FROM_MASTER_DATA);
                var reqMatrixB = com.ImmediateSend(matrixB, destination, FROM_MASTER_MATRIX_B);

                reqOffsetRow.Wait();
                reqRows.Wait();
                reqData.Wait();
                reqMatrixB.Wait();

                offsetRow += rows;
            }
        }

        private void StartSlave()
        {
            var com = Communicator.world;
            int source = 0;

            var offsetRow = com.ImmediateReceive<int>(source, FROM_MASTER_OFFSET_ROWS);
            var rows = com.ImmediateReceive<int>(source, FROM_MASTER_ROWS);
            var mA = com.ImmediateReceive<double[][]>(source, FROM_MASTER_DATA);
            var mB = com.ImmediateReceive<double[][]>(source, FROM_MASTER_MATRIX_B);
           
            offsetRow.Wait();
            rows.Wait();
            mA.Wait();
            mB.Wait();
            
            var mC = MatrixService.MultiplyMatrix((double[][])mA.GetValue(), (double[][])mB.GetValue());

            var reqOffsetRow = com.ImmediateSend((int)offsetRow.GetValue(), source, FROM_WORKER_OFFSET_ROWS);
            var reqRows = com.ImmediateSend((int)rows.GetValue(), source, FROM_WORKER_ROWS);
            var reqResult = com.ImmediateSend(mC, source, FROM_WORKER_RESULT);

            reqOffsetRow.Wait();
            reqRows.Wait();
            reqResult.Wait();
        }

    }
}
