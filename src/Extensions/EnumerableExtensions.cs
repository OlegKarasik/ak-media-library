using System.Runtime.InteropServices;

namespace MediaLibrary.Extensions;

public static class StringExtensions
{
  public static int CalculateLevenshteinDistance(
    this string @this, 
    string right)
  {
    var memory = new Dictionary<(int, int), int>();

    var r =  Recursion(memory, @this, right, @this.Length, right.Length);

    return r;

    static int Recursion(Dictionary<(int, int), int> memory, string x, string y, int m, int n)
    {
      if (m == 0)
      {
        return n;
      }
      if (n == 0)
      {
        return m;
      }

      if (x[m - 1] == y[n - 1])
      {
        return memory.TryGetValue((m - 1, n - 1), out var result)
          ? result
          : memory[(m - 1, n - 1)] = Recursion(memory, x, y, m - 1, n - 1);
      }

      var insertion = memory.TryGetValue((m, n - 1), out var insertionResult) 
        ? insertionResult
        : memory[(m, n - 1)] = Recursion(memory, x, y, m, n - 1);

      var removal = memory.TryGetValue((m - 1, n), out var removalResult) 
        ? removalResult
        : memory[(m - 1, n)] = Recursion(memory, x, y, m - 1, n);

      var replacement = memory.TryGetValue((m - 1, n - 1), out var replacementResult) 
        ? replacementResult
        : memory[(m - 1, n - 1)] = Recursion(memory, x, y, m - 1, n - 1);

      return 1 + Math.Min(Math.Min(insertion, removal), replacement);
    }
  }
}

public static class EnumerableExtensions
{
  public static Dictionary<string, T> CollideMany<S, T>(
    this IEnumerable<S> source,
    Func<S, IEnumerable<T>> manyFunc,
    Func<T, string> keyFunc)
  {
    return source.SelectMany(s => manyFunc(s)).Collide(keyFunc);
  }

  public static Dictionary<string, T> Collide<T>(
    this IEnumerable<T> @this,
    Func<T, string> keyFunc)
  {
    if (keyFunc is null)
    {
      throw new ArgumentNullException(nameof(keyFunc));
    }

    Dictionary<string, T> result = [];
    Dictionary<string, int> indices = [];
    foreach (var item in @this.Order())
    {
      var key = keyFunc(item);
      ref int index = ref CollectionsMarshal.GetValueRefOrAddDefault(indices, key, out var exists);
      if (!exists)
      {
        result[key] = item;
        continue;
      }
      if (index == default)
      {
        index++;
        if (result.Remove(key, out var current))
        {
          result[$"{key} ({index})"] = current;
        }
      }
      index++;
      result[$"{key} ({index})"] = item;
    }
    return result;
  }
}
