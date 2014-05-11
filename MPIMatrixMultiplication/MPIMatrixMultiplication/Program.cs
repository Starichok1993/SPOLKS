using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace MPIMatrixMultiplication
{
    class Program
    {

        private static void Main(string[] args)
        {
            var manager = new MPIManager(10, 10, 10);
            manager.Start(ref args);
        }
    }
}
