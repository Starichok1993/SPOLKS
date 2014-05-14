#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <mpi.h>


const int FROM_MASTER = 1;
const int FROM_WORKER = 2;

struct Matrix
{
  double* data;
  int rowsCount;
  int columnsCount;
} matrixA, matrixB, matrixC;

Matrix CreateMatrix(double* data, int rowsCount, int columnsCount);
void CreateMatrix(Matrix* matrix, int rowsCount, int columnsCount);
void PrintMatrix(Matrix matrix);
void RandomInitialization(Matrix* matrix);
Matrix MultiplyMatrix(Matrix matrixA, Matrix matrixB);
void DeleteMatrix(Matrix matrix);

void StartSlave(MPI_Comm communicator, int indentity);
void StartMaster(MPI_Comm communicator, int indentity);
void ShareTheTask(MPI_Comm communicator, int numberOfSlaves);
void GatherResults(MPI_Comm communicator, int numberOfSlaves);
void Init();

int main (int argc, char* argv[])
{
  int errCode;

  if ((errCode = MPI_Init(&argc, &argv)) != 0)
  {
    return errCode;
  }

  int currentTaskId;

  MPI_Comm_rank(MPI_COMM_WORLD, &currentTaskId);

  if (currentTaskId == 0)
  {    
    Init();
    StartMaster(MPI_COMM_WORLD, 0);

    printf("-----------------------\n");
    //PrintMatrix(matrixC);
    
    DeleteMatrix(matrixA);
    DeleteMatrix(matrixB);
    DeleteMatrix(matrixC);
  }
  if (currentTaskId > 0)
  {
    StartSlave(MPI_COMM_WORLD, 0);
  }

  MPI_Finalize();
  return 0;
}

void Init()
{
  CreateMatrix(&matrixA, 5, 5);
  CreateMatrix(&matrixB, 5, 5);
  CreateMatrix(&matrixC, matrixA.rowsCount, matrixB.columnsCount);

  RandomInitialization(&matrixA);
  RandomInitialization(&matrixB); 
}

void StartMaster(MPI_Comm communicator, int indentity)
{ 
  int numberOfSlaves;
  MPI_Comm_size(communicator, &numberOfSlaves);

  printf("Group: %d Number of worker = %d\n", indentity, --numberOfSlaves);

  PrintMatrix(matrixA);
  PrintMatrix(matrixB);
  double start = MPI_Wtime();

  ShareTheTask(communicator, numberOfSlaves);
  GatherResults(communicator, numberOfSlaves);

  printf("_________________________________________________\n");
  PrintMatrix(matrixC);
  double stop = MPI_Wtime();
  printf("Group: %d Worked time = %0.5f\n", indentity, stop - start);
}

void ShareTheTask(MPI_Comm communicator, int numberOfSlaves)
{
  int rowsPerWorker = matrixA.rowsCount / numberOfSlaves;
  int remainingRows = matrixA.rowsCount % numberOfSlaves;
  int offsetRow = 0;
  int messageType = FROM_MASTER;

  printf("Remaining rows: %d\n", remainingRows);
  for (int destination = 1; destination <= numberOfSlaves; destination++)
  {
    int rows = (destination <= remainingRows) ? rowsPerWorker + 1 : rowsPerWorker;

    printf ("Rows: %d to worker: %d\n", rows, destination);

    MPI_Send( &offsetRow, 1, MPI_INT, destination, FROM_MASTER, communicator);
    MPI_Send( &rows, 1, MPI_INT, destination, FROM_MASTER, communicator);
    MPI_Send( &(matrixA.columnsCount), 1, MPI_INT, destination, FROM_MASTER, communicator);
    
    double* temp = (double*) malloc(sizeof(double) * rows * matrixA.columnsCount);
    for (int i = 0; i < rows; i++)
    {
      for (int j = 0; j < matrixA.columnsCount; j++)
      {
        temp[i * matrixA.columnsCount + j] = matrixA.data[(offsetRow + i) * matrixA.columnsCount + j];
      }
    }
    
    MPI_Send( temp, rows * matrixA.columnsCount, MPI_DOUBLE, destination, FROM_MASTER, communicator);
    MPI_Send( &(matrixB.columnsCount), 1, MPI_INT, destination, FROM_MASTER, communicator);
    MPI_Send( matrixB.data, matrixB.columnsCount * matrixB.rowsCount, MPI_DOUBLE, destination, FROM_MASTER, communicator);

    offsetRow += rows;
  }
}

void GatherResults(MPI_Comm communicator, int numberOfSlaves)
{
  for (int source = 1; source <= numberOfSlaves; source++)
  {
    MPI_Status status;
    int rowOffset;
    MPI_Recv( &rowOffset, 1, MPI_INT, source, FROM_WORKER, communicator, &status);

    int rows;
    MPI_Recv( &rows, 1, MPI_INT, source, FROM_WORKER, communicator, &status);

    double* buf = (double*) malloc (sizeof(double) * rows * matrixC.columnsCount);
    MPI_Recv(buf, rows * matrixC.columnsCount, MPI_DOUBLE, source, FROM_WORKER, communicator, &status);

    for (int i = 0; i < rows; i++)
    {
      for (int j = 0; j < matrixC.columnsCount; j++)
      {
        matrixC.data[(rowOffset + i) * matrixC.columnsCount + j] = buf[i * matrixC.columnsCount + j];
      }
    }
  }

}

void StartSlave(MPI_Comm communicator, int indentity)
{
  MPI_Status status;

  int offsetRow;
  MPI_Recv( &offsetRow, 1, MPI_INT, 0, FROM_MASTER, communicator, &status);
  
  int rows;
  MPI_Recv(&rows, 1, MPI_INT, 0, FROM_MASTER, communicator, &status);

  int mAColumnsCount;
  MPI_Recv(&mAColumnsCount, 1, MPI_INT, 0, FROM_MASTER, communicator, &status);

  double* bufA = (double*) malloc(sizeof(double) * rows * mAColumnsCount);
  MPI_Recv(bufA, rows * mAColumnsCount, MPI_DOUBLE, 0, FROM_MASTER, communicator, &status);
  
  int mBColumnsCount;
  MPI_Recv(&mBColumnsCount, 1, MPI_INT, 0, FROM_MASTER, communicator, &status);

  double* bufB = (double*) malloc(sizeof(double) * mAColumnsCount * mBColumnsCount);
  MPI_Recv( bufB, mAColumnsCount * mBColumnsCount, MPI_DOUBLE, 0, FROM_MASTER, communicator, &status);  

  matrixA = CreateMatrix(bufA, rows, mAColumnsCount);
  matrixB = CreateMatrix(bufB, mAColumnsCount, mBColumnsCount);
  
  matrixC = MultiplyMatrix( matrixA, matrixB);

  MPI_Send( &offsetRow, 1, MPI_INT, 0, FROM_WORKER, communicator);
  MPI_Send( &rows, 1, MPI_INT, 0, FROM_WORKER, communicator);
  MPI_Send( matrixC.data, matrixC.columnsCount * matrixC.rowsCount, MPI_DOUBLE, 0, FROM_WORKER, communicator);
}




//Region Matrix Service

Matrix CreateMatrix(double* data, int rowsCount, int columnsCount)
{
  Matrix result;
  result.columnsCount = columnsCount;
  result.rowsCount = rowsCount;
  result.data = data;

  return result;
}

void CreateMatrix(Matrix* matrix, int rowsCount, int columnsCount)
{
  matrix->data = (double*) malloc(sizeof(double) * rowsCount * columnsCount);

  /*for(int i = 0; i < rowsCount; i++)
  {
    matrix->data[i] = (double*) malloc(sizeof(double) * columnsCount);
  }*/

  matrix->columnsCount = columnsCount;
  matrix->rowsCount = rowsCount;
}

void PrintMatrix(Matrix matrix)
{
  for (int i = 0; i < matrix.rowsCount; i++)
  {
    for (int j = 0; j < matrix.columnsCount; j++)
    {
      printf("%.2f ", matrix.data[i*matrix.columnsCount + j]);
    }
    printf("\n");
  }
}

void RandomInitialization(Matrix* matrix)
{
  for (int i = 0; i < matrix->rowsCount; i++)
  {
    for (int j = 0; j < matrix->columnsCount; j++)
    {
      matrix->data[i * matrix->columnsCount + j] = 1.0;
    }
  }
}

Matrix MultiplyMatrix(Matrix matrixA, Matrix matrixB)
{
  Matrix result;
  CreateMatrix(&result, matrixA.rowsCount, matrixB.columnsCount);

  double sum;
  for (int i = 0; i < matrixA.rowsCount; i++)
  {
    for (int j = 0; j < matrixB.columnsCount; j++)
    {
      sum = 0;
      for (int k = 0; k < matrixA.columnsCount; k++)
      {
        sum += matrixA.data[i * matrixA.columnsCount + k] * matrixB.data[k * matrixB.columnsCount + j];
      }
      result.data[i * result.columnsCount + j] = sum;
    }
  }

  return result;
}

void DeleteMatrix(Matrix matrix)
{
  /*for (int i = 0; i < matrix.rowsCount; i++)
  {
    free(matrix.data[i]);
  }*/
  free(matrix.data);
}

//End Region Matrix Service

