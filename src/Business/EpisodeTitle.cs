using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(value)},nq}}")]
public partial class EpisodeTitle
{
  private static readonly Regex standard = MatchStandardParts();
  private static readonly Regex complex = MatchComplexParts();
  private static readonly Regex normalise = MatchNormaliseParts();

  private readonly string value;

  [JsonConstructor]
  public EpisodeTitle(
    string value)
  {
    ArgumentException.ThrowIfNullOrEmpty(value);

    var intermediate = value.EscapeInvalidCharacters();
    intermediate = standard.Replace(
      intermediate,
      match =>
      {
        return $"({match.Groups["Index"].Value})";
      });
    
    intermediate = complex.Replace(
      intermediate,
      match =>
      {
        return $"({match.Groups["Index"].Value})";
      });
    
    intermediate = complex.Replace(
      intermediate,
      match =>
      {
        return $"({match.Groups["Index"].Value})";
      });
    
    intermediate = normalise.Replace(
      intermediate,
      match =>
      {
        return $" ({match.Groups["Index"].Value})";
      });

    if (intermediate.Contains("&&"))
    {
      intermediate = string.Join(" && ", intermediate.Split(" && ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }

    this.value = intermediate;
  }

  public override string ToString()
  {
    return this.value;
  }

  public override bool Equals(object? obj)
  {
    return obj is string s && s.Equals(this.value, StringComparison.OrdinalIgnoreCase);
  }

  public override int GetHashCode()
  {
    return this.value.GetHashCode(StringComparison.OrdinalIgnoreCase);
  }

  [GeneratedRegex(@"\(?(Part|Chapter|Volume)\s*(?<Index>\d+)\)?")]
  private static partial Regex MatchStandardParts();

  [GeneratedRegex(@"\(?(?<Index>\d+)(st|nd|th)\s*(Part|Chapter|Volume)\)?")]
  private static partial Regex MatchComplexParts();

  [GeneratedRegex(@"\s*-?\s*\((?<Index>\d+)\)")]
  private static partial Regex MatchNormaliseParts();
}
