using System.Diagnostics;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(value)},nq}}")]
public abstract class MediaString : IEquatable<MediaString>
{
  private readonly string value;

  public MediaString(
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    this.value = value.EscapeInvalidCharacters().Trim();
  }

  public override bool Equals(
    object? obj)
  {
    return this.Equals(obj is MediaTitle);
  }

  public override int GetHashCode()
  {
    return this.value.GetHashCode();
  }

  public override string ToString()
  {
    return this.value;
  }

  public string ToString(
    MediaStringPresentation presentation)
  {
    return presentation switch
    {
      MediaStringPresentation.AllCaps => this.value.ToUpper(),
      _ => this.value
    };
  }

  public bool Equals(
    MediaString? other)
  {
    return other is not null && this.value.Equals(other.value, StringComparison.Ordinal);
  }

  public static implicit operator string(MediaString input) 
  {
    return input.value;
  }
}
