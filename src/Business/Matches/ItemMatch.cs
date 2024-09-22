using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public abstract class ItemMatch
{
  protected T? Get<T>(
    Match match,
    string key)
  {
    var value = match.Groups.ContainsKey(key) 
      ? match.Groups[key].Value
      : null;

    if (value is null)
    {
      return default;
    }

    if (typeof(T) == typeof(string))
    {
      return (T)(object)value;
    }
    if (typeof(T) == typeof(long))
    {
      return (T)(object)long.Parse(value);
    }
    return default;
  }
}
