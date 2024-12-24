using System.Runtime.InteropServices;

namespace MediaLibrary.Extensions;

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
