namespace MediaLibrary.Business;

public class PropsFilePath : FilePath
{
  public PropsFilePath(
    FilePath value)

    : base(NormalizePath(value))
  {
  }

  public PropsFilePath(
    DirectoryPath value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    FilePath value)
  {
    const string PROPS_POSTFIX = ".props.json";

    if (value.Value.EndsWith(PROPS_POSTFIX))
    {
      return value.Value;
    }
    return string.Concat(value, PROPS_POSTFIX);
  }

  private static string NormalizePath(
    DirectoryPath path)
  {
    const string PROPS_FILE = "this.props.json";

    if (path.Value.EndsWith(PROPS_FILE))
    {
      return path.Value;
    }
    return Path.Combine(path.Value, PROPS_FILE);
  }
}
