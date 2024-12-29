namespace MediaLibrary.Extensions;

public class SafeNameEqualityComparer : IEqualityComparer<string>
{
  public static readonly SafeNameEqualityComparer Default = new();

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
    return StringComparer.OrdinalIgnoreCase.Equals(
      x.EscapeInvalidCharacters(), y.EscapeInvalidCharacters());
  }

  public int GetHashCode(
    string obj)
  {
    return StringComparer.OrdinalIgnoreCase.GetHashCode(
      obj.EscapeInvalidCharacters());
  }
}
