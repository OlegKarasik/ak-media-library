using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Items;

public abstract class ItemMatch
{
  private static T Convert<T>(
    string value)
  {
    if (typeof(T) == typeof(string))
    {
      return (T)(object)value;
    }
    if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
    {
      return (T)(object)long.Parse(value);
    }
    if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
    {
      return (T)(object)int.Parse(value);
    }
    throw new NotImplementedException();
  }

  protected static T Required<T>(
    Match match,
    string key)
  {
    return match.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : throw new Exception($"The match must include \"{key}\" capture group");
  }

  protected static T? Optional<T>(
    Match match,
    string key)
  {
    return match.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : default;
  }
}
