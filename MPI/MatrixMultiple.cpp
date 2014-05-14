#include <stdio.h>
#include <getopt.h>
#include <stdlib.h>
#include <mpi.h>
#include <sys/time.h>

#define FROM_MASTER 10
#define FROM_SLAVE 20

struct TMatrix
{
	int rows, columns;
	double* data;
};

TMatrix createMatrix(int rows, int columns)
{
	TMatrix matrix;
	matrix.rows = rows;
	matrix.columns = columns;
	matrix.data = (double *) malloc(rows * columns * sizeof(double));
	return matrix;
}


void printMatrix(TMatrix matrix)
{
	for(int i = 0; i < matrix.rows; i++)
	{
		for(int j = 0; j < matrix.columns; j++)
		{
			printf("%.2f ", matrix.data[i * matrix.rows + j]);
		}
		printf("\n");
	}
}

void fillSimpleMatrix(TMatrix* matrix)
{
	for(int i = 0; i < matrix->rows; i++)
	{
		for(int j = 0; j < matrix->columns; j++)
		{
			matrix->data[i * matrix->rows + j] = 1.0;
		}
	}
}



int main(int argc, char** argv)
{
	MPI_Init(&argc, &argv);
	int size, rank;
	MPI_Comm_size(MPI_COMM_WORLD, &size);
	MPI_Comm_rank(MPI_COMM_WORLD, &rank);

	//printf("I am process number - %d\n", rank);

	int rowsA = 4;
	int columnA = 3;
	int columnB = 4;
	int rowsB = columnA;
	TMatrix matrixC = createMatrix(rowsA, columnB);

	if(rank == 0)
	{
		double start, end;
		TMatrix matrixA = createMatrix(rowsA, columnA);
		fillSimpleMatrix(&matrixA);

		TMatrix matrixB = createMatrix(rowsB, columnB);
		fillSimpleMatrix(&matrixB);
		
		printMatrix(matrixA);
		printf("\n");
		printMatrix(matrixB);
		printf("\n");

		int numberOfSlaves = size - 1;
		start = MPI_Wtime();

		int rowsPerWorker = rowsA / numberOfSlaves;
		int remainingRows = rowsA % numberOfSlaves;
		int offsetRow = 0;
		int messageType = FROM_MASTER;
		for(int destination = 1; destination <= numberOfSlaves; destination++)
		{
			int rows = (destination <= remainingRows) ? rowsPerWorker + 1 : rowsPerWorker;
			MPI_Send((void *)&offsetRow, 1, MPI_INT, destination, messageType, MPI_COMM_WORLD);
			MPI_Send((void *)&rows, 1, MPI_INT, destination, messageType, MPI_COMM_WORLD);

			double* temp = (double *) malloc(sizeof(double) * rows * columnA);
			temp = matrixA.data + offsetRow;


			MPI_Send((void *)temp, rows * columnA, MPI_DOUBLE, destination, messageType, MPI_COMM_WORLD);
			// MPI_Send((void *)matrixB.data, columnA * columnB, MPI_DOUBLE, destination, messageType, MPI_COMM_WORLD);
			offsetRow += rows;
		}

		// messageType = FROM_SLAVE;

		// for(int source = 1; source <= numberOfSlaves; source++)
		// {
		// 	int rowOffset;
		// 	MPI_Recv((void*)&rowOffset, 1, MPI_INT, source, messageType, MPI_COMM_WORLD, NULL);
			
		// 	int rows;
		// 	MPI_Recv((void*)&rows, 1, MPI_INT, source, messageType, MPI_COMM_WORLD, NULL);

		// 	double** temp;
		// 	MPI_Recv((void*)temp, rows * offsetRow, MPI_DOUBLE

		// 	for(int j = 0; j < rows; j++)
		// 	{
		// 		matrixC.data[rowOffset + j] = 
		// 	}
		// }
	}
	else
	{
		int offsetRow;
		MPI_Status status;
		MPI_Recv((void*)&offsetRow, 1, MPI_INT, 0, FROM_MASTER, MPI_COMM_WORLD, &status);

		int rows;
		MPI_Recv((void*)&rows, 1, MPI_INT, 0, FROM_MASTER, MPI_COMM_WORLD, &status);
		printf("Process - %d, Offset - %d, Rows - %d\n", rank, offsetRow, rows);

		TMatrix tMatrix = createMatrix(rows, columnA);
		MPI_Recv((void*)tMatrix.data, rows * columnA, MPI_DOUBLE, 0, FROM_MASTER, MPI_COMM_WORLD, &status);

		printMatrix(tMatrix);
		printf("\n");
	}

	MPI_Finalize();
	return 0;	
}