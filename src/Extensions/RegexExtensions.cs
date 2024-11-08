using System.Text.RegularExpressions;

namespace MediaLibrary.Extensions;

public static class RegexExtensions
{
  public static T Required<T>(
    this Match @this,
    string key)
  {
    return @this.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : throw new InvalidOperationException($"The match must include \"{key}\" capture group");
  }

  public static T? Optional<T>(
    this Match @this,
    string key)
  {
    return @this.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : default;
  }

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
}