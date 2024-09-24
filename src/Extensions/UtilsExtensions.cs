namespace MediaLibrary.Extensions;

public static class UtilityExtensions
{
  public static void UtilzInsertRange<K, T>(
    this Dictionary<K,  List<T>> @this,
    IEnumerable<T> values,
    Func<T, K> keyFunc)

    where K : notnull
  {
    if (@this is null)
    {
      throw new ArgumentNullException(nameof(@this));
    }
    if (values is null)
    {
      throw new ArgumentNullException(nameof(values));
    }
    if (keyFunc is null)
    {
      throw new ArgumentNullException(nameof(keyFunc));
    }

    foreach (var value in values)
    {
      var key = keyFunc(value);
      if (!@this.TryGetValue(key, out var list))
      {
        @this[key] = list ??= [];
      }
      list.Add(value);
    }
  }

  public static void UtilzMergeRange<K, T>(
    this Dictionary<K, List<T>> @this,
    Dictionary<K, List<T>> source)

    where K : notnull
  {
    if (@this is null)
    {
      throw new ArgumentNullException(nameof(@this));
    }
    if (source is null)
    {
      throw new ArgumentNullException(nameof(source));
    }

    foreach (var (key, values) in source)
    {
      if (!@this.TryGetValue(key, out var list))
      {
        @this[key] = list ??= [];
      }
      list.AddRange(values);
    }
  }
}