using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPIMatrixMultiplication;

namespace FileToConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var key = Console.ReadKey();
                if (key.KeyChar != 'q')
                {
                    Console.Clear();
                    var count = 4;
                    for (int i = 0; i < count; i++)
                    {
                        Console.WriteLine("Matrix {0}", i);
                        var matrix = MatrixService.ReadMatrix(@"C:\Users\Alex\Documents\Visual Studio 2012\Projects\SPOLKS\MPIMatrixMultiplication\MPIMatrixMultiplication\bin\Debug\saveFileGroup" + i);
                        MatrixService.PrintMatrix(matrix);
                    }
                }
            }
        }
    }
}
