using System.Runtime.CompilerServices;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Enrichment.Common;

public class EpisodeTitleEqualityComparer : IEqualityComparer<string>
{
  public static readonly EpisodeTitleEqualityComparer Default = new();

  public bool Equals(
    string? x,
    string? y)
  {
    if (object.ReferenceEquals(x, y))
    {
      return true;
    }
    if (x is null || y is null)
    {
      return false;
    }

    return StringComparer.OrdinalIgnoreCase.Equals(ProcessString(x), ProcessString(y));
  }

  public int GetHashCode(
    string obj)
  {
    return StringComparer.OrdinalIgnoreCase.GetHashCode(ProcessString(obj));
  }

  private static string ProcessString(
    string s)
  {
    return new string([.. s.EscapeInvalidCharacters().Where(i => !char.IsWhiteSpace(i))]);
  }
}
