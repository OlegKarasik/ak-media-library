namespace MediaLibrary.Business;

public class PropsPath
{
  private const string PROPS_SUFFIX = ".props.json";

  public string Value { get; }

  public PropsPath(
    FilePath value)
  {
    if (value is null)
    {
      throw new ArgumentNullException(nameof(value));
    }

    this.Value = $"{value.Value}{PROPS_SUFFIX}";
  }

  public PropsPath(
    DirectoryPath value)
  {
    if (value is null)
    {
      throw new ArgumentNullException(nameof(value));
    }

    this.Value = Path.Combine(value.Value, $"this{PROPS_SUFFIX}");
  }

  public static bool IsPropsPath(
    FilePath value)
  {
    if (value is null)
    {
      throw new ArgumentNullException(nameof(value));
    }

    return value.Value.EndsWith(PROPS_SUFFIX);
  }
}
