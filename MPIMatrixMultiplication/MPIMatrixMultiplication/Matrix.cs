using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPIMatrixMultiplication
{
    public class Matrix
    {
        public double[][] Data { get; set; }

        public int ColumnsCount { get; private set; }

        public int RowsCount { get; set; }

        public Matrix(uint nColumns, uint nRows)
        {
            Init(nColumns, nRows);
        }
        
        private void Init(uint nColumns, uint nRows)
        {
            ColumnsCount = (int) nColumns;
            RowsCount = (int) nRows;
            Data = new double[nRows][];

            for (int i = 0; i < nRows; i++)
            {
                Data[i] = new double[nColumns];
            }
        }

        public void RandomInitialization()
        {
            var random = new Random();
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    Data[i][j] = random.NextDouble();
                }
            }
        }
    }
}
