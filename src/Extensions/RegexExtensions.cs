using System.Text.RegularExpressions;

namespace MediaLibrary.Extensions;

public static class RegexExtensions
{
  public static T Required<T>(
    this Match @this,
    string key)
  {
    if (string.IsNullOrEmpty(key))
    {
      throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
    }

    return @this.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : throw new InvalidOperationException($"The match must include \"{key}\" capture group");
  }

  public static T? Optional<T>(
    this Match @this,
    string key)
  {
    if (key is null)
    {
      throw new ArgumentNullException(nameof(key));
    }

    return @this.Groups.TryGetValue(key, out var group) 
      ? Convert<T>(group.Value) 
      : default;
  }

  public static ReadOnlySpan<char> Optional(
    this Match @this,
    string key)
  {
    if (key is null)
    {
      throw new ArgumentNullException(nameof(key));
    }

    if (@this.Groups.TryGetValue(key, out var group))
    {
      return group.ValueSpan;
    }
    return default;
  }

  private static T Convert<T>(
    string value)
  {
    if (string.IsNullOrEmpty(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
    }

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
    if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
    {
      return (T)(object)short.Parse(value);
    }
    if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?))
    {
      return (T)(object)ulong.Parse(value);
    }
    if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?))
    {
      return (T)(object)uint.Parse(value);
    }
    if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?))
    {
      return (T)(object)ushort.Parse(value);
    }
    if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
    {
      return (T)(object)byte.Parse(value);
    }
    throw new NotImplementedException();
  }
}