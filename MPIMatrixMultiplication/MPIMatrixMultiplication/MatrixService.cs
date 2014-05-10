using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MPIMatrixMultiplication
{
    public class MatrixService
    {

        public static void CreateMatrix(out double[][] matrix, int rows, int columns)
        {
            matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new double[columns];
            }
        }

        //public static Matrix ReadMatrix(string fileName)
        //{
        //    return null;
        //}

        //public static bool ValidMatrix(Matrix matrixA, Matrix matrixB)
        //{
        //    if (matrixA.ColumnsCount != matrixB.RowsCount)
        //        return true;

        //    return false;
        //}

        //public static Matrix MultiplyMatrix(Matrix matrixA, Matrix matrixB)
        //{
        //    if (!ValidMatrix(matrixA, matrixB))
        //        return null;
            
        //    var matrixC = new Matrix((uint)matrixB.ColumnsCount, (uint)matrixA.RowsCount);

        //    double sum;
        //    for (var i = 0; i < matrixA.ColumnsCount; i++)
        //    {
        //        for (var j = 0; j < matrixB.RowsCount; j++)
        //        {
        //            sum = 0;
        //            for (var k = 0; k < matrixA.RowsCount; k++)
        //            {
        //                sum += matrixA.Data[i][k] * matrixB.Data[k][j];
        //            }
        //            matrixC.Data[i][j] = sum;
        //        }
        //    }
            
        //    return matrixC;
        //}

        public static double[][] MultiplyMatrix(double[][] matrixA, double[][] matrixB)
        {
            double[][] matrixC = new double[matrixA.Length][];
            for (int i = 0; i < matrixA.Length; i++)
            {
                matrixC[i] = new double[matrixB[0].Length];
            }

            double sum;
            for (var i = 0; i < matrixA.Length; i++)
            {
                for (var j = 0; j < matrixB[0].Length; j++)
                {
                    sum = 0;
                    for (var k = 0; k < matrixA[0].Length; k++)
                    {
                        sum += matrixA[i][k] * matrixB[k][j];
                    }
                    matrixC[i][j] = sum;
                }
            }

            return matrixC;
        }

        public static void PrintMatrix(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    Console.Write("{0:0.##} ",matrix[i][j]);
                }

                Console.WriteLine("");
            }
        }

        public static void RandomInitialization(double[][] data)
        {
            var random = new Random();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[0].Length; j++)
                {
                    data[i][j] = random.NextDouble();
                }
            }
        }

        public static double[][] ReadMatrix(string fileName)
        {
            var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[sizeof(int)];
            file.Read(buffer, 0, sizeof (int));
            var rowsCount = BitConverter.ToInt32(buffer, 0);
            file.Read(buffer, 0, sizeof (int));
            var columnsCount = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[sizeof(double)];
            double[][] result;
            CreateMatrix(out result, rowsCount, columnsCount);

            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                {
                    file.Read(buffer, 0, sizeof (double));
                    result[i][j] = BitConverter.ToDouble(buffer, 0);
                }
            }

            return result;
        }

        public static void WriteMatrix(string fileName, double[][] matrix, int offsetRows)
        {
            var rowsCount = matrix.Length;
            var columnsCount = matrix[0].Length;

            var startOffset = sizeof (int) * 2;
            var file = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.Write);

            file.Seek(startOffset + (sizeof(double)) * ((offsetRows) * columnsCount), SeekOrigin.Begin);
            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                {
                    file.Write(BitConverter.GetBytes(matrix[i][j]), 0, sizeof(double));
                }
            }

            file.Close();
        }
    }
}
