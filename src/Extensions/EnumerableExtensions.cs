namespace MediaLibrary.Extensions;

public static class EnumerableExtensions
{
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
      if (!result.TryAdd(key, item))
      {
        if (!indices.TryGetValue(key, out var index))
        {
          index = 1;

          if (result.Remove(key, out var current))
          {
            result[$"{key} ({index++})"] = current;
          }
        }

        if (!result.TryAdd($"{key} ({index++})", item))
        {
          throw new Exception();
        }

        indices[key] = index;
      }  
    }
    return result;
  }
}
