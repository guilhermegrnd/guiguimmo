namespace Guiguimmo.Utils;

public static class Helper
{
  public static int[][] ConvertRectangularToJagged(int[,] rectangularArray)
  {
    int height = rectangularArray.GetLength(0);
    int width = rectangularArray.GetLength(1);

    int[][] jaggedArray = new int[height][];

    for (int y = 0; y < height; y++)
    {
      jaggedArray[y] = new int[width];
      for (int x = 0; x < width; x++)
      {
        jaggedArray[y][x] = rectangularArray[y, x];
      }
    }
    return jaggedArray;
  }
}