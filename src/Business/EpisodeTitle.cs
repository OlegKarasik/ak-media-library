using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Value)},nq}}")]
public partial class EpisodeTitle
{
  private static readonly Regex standard  = MatchStandardParts();
  private static readonly Regex complex   = MatchComplexParts();
  private static readonly Regex normalise = MatchNormaliseParts();

  [JsonInclude]
  public string Value
  {
    get; private set;
  }

#pragma warning disable CS8618

  [JsonConstructor]
  protected EpisodeTitle()
  {
  }

#pragma warning restore CS8618
  
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
      intermediate = string.Join(" && ", intermediate.Split("&&", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
    if (intermediate.Contains(''))
    {
      intermediate = string.Join(" && ", intermediate.Split("", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
    intermediate = intermediate.Trim();

    this.Value = intermediate;
  }

  public override string ToString()
  {
    return this.Value;
  }

  public override bool Equals(object? obj)
  {
    return obj is EpisodeTitle t && t.Value.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
  }

  public override int GetHashCode()
  {
    return this.Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
  }

  [GeneratedRegex(@"\(?(Part|Chapter|Volume)\s*(?<Index>\d+)\)?")]
  private static partial Regex MatchStandardParts();

  [GeneratedRegex(@"\(?(?<Index>\d+)(st|nd|th)\s*(Part|Chapter|Volume)\)?")]
  private static partial Regex MatchComplexParts();

  [GeneratedRegex(@"\s*-?\s*\((?<Index>\d+)\)")]
  private static partial Regex MatchNormaliseParts();
}
