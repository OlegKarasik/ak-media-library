using System.Buffers;
using System.Text;

namespace MediaLibrary.Extensions;

public static class StringExtensions
{
  private ref struct LevensteinDistanceMemory
  {
    private Span<int> memory;
    private int rows;
    private int cols;

    public LevensteinDistanceMemory(
      Span<int> memory,
      int rows,
      int cols)
    {
      this.memory = memory;
      this.rows = rows;
      this.cols = cols;
    }

    public ref int At(int x, int y)
    {
      return ref memory[x * this.cols + y];
    }
  }

  private static readonly SearchValues<char> invalidCharacters;

  static StringExtensions()
  {
    invalidCharacters = SearchValues.Create(Path.GetInvalidFileNameChars());
  }

  public static int CalculateLevenshteinDistance(
    this string @this, 
    string other)
  {
    if (string.IsNullOrEmpty(other))
    {
      return @this.Length;
    }

    var memorySize = (@this.Length + 1) * (other.Length + 1);

    Span<int> memory = memorySize < 1024
      ? stackalloc int[memorySize]
      : new int[memorySize];

    memory.Fill(-1);

    return Recursion(
      new LevensteinDistanceMemory(memory, @this.Length, other.Length), 
      @this, 
      other, 
      @this.Length, 
      other.Length);

    static int Recursion(
      LevensteinDistanceMemory memory, 
      string x, 
      string y, 
      int m, 
      int n)
    {
      if (m == 0)
      {
        return n;
      }
      if (n == 0)
      {
        return m;
      }
      if (memory.At(m, n) != -1)
      {
        return memory.At(m, n);
      }

      if (char.ToLower(x[m - 1]) == char.ToLower(y[n - 1]))
      {
        return Recursion(memory, x, y, m - 1, n - 1);
      }
      return memory.At(m, n) 
        = 1 + new int[] { Recursion(memory, x, y, m, n - 1), Recursion(memory, x, y, m - 1, n), Recursion(memory, x, y, m - 1, n - 1) }.Min();
    }
  }

  public static string EscapeInvalidCharacters(
    this string @this)
  {
    if (@this.AsSpan().ContainsAny(invalidCharacters))
    {
      var xb = new StringBuilder(@this);
      foreach (var c in Path.GetInvalidFileNameChars())
      {
        xb.Replace(c.ToString(), string.Empty);
      }
      return xb.ToString();
    }
    return @this;
  }
}
