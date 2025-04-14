using MediaLibrary.Extensions;

namespace MediaLibrary.Business;

public class MediaTitle : MediaString
{
  public MediaTitle(
    string value)

    : base(Init(value))
  {
  }

  private static string Init(
    string? value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);
    
    return value.EscapeInvalidCharacters();
  }
}
