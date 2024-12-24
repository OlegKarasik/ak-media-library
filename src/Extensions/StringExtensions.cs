namespace MediaLibrary.Extensions;

public static class StringExtensions
{
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

    return Recursion(memory, other.Length, @this, other, @this.Length, other.Length);

    static int Recursion(
      Span<int> memory, 
      int sz,
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

      if (memory[m * sz + n] != -1)
      {
        return memory[m * sz + n];
      }

      if (char.ToLower(x[m - 1]) == char.ToLower(y[n - 1]))
      {
        return Recursion(memory, sz, x, y, m - 1, n - 1);
      }

      return memory[m * sz + n] 
        = 1 + 
          Math.Min(
            Math.Min(
              Recursion(memory, sz, x, y, m, n - 1), 
              Recursion(memory, sz, x, y, m - 1, n)), 
            Recursion(memory, sz, x, y, m - 1, n - 1));
    }
  }
}
