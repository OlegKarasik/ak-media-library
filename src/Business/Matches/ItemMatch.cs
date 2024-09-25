using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public abstract class ItemMatch
{
  private static T Convert<T>(
    string value)
  {
    if (typeof(T) == typeof(string))
    {
      return (T)(object)value;
    }
    if (typeof(T) == typeof(long))
    {
      return (T)(object)long.Parse(value);
    }
    throw new NotImplementedException();
  }

  protected static T Required<T>(
    Match match,
    string key)
  {
    return match.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : throw new Exception();
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
