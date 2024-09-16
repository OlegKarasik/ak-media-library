namespace MediaLibrary.Business;

public class PropsPath
{
  private const string PROPS_SUFFIX = ".props.json";

  private readonly string value;

  public PropsPath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.value = Path.EndsInDirectorySeparator(value) || !Path.HasExtension(value)
      ? Path.Combine(value, $"this{PROPS_SUFFIX}") 
      : $"{value}{PROPS_SUFFIX}";
  }

  public static explicit operator string(PropsPath x)
  {
    return x.value;
  }
}
