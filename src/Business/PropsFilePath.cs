namespace MediaLibrary.Business;

public class PropsFilePath : FilePath
{
  public PropsFilePath(
    FilePath path)

    : base(NormalizePath(path))
  {
  }

  public PropsFilePath(
    DirectoryPath path)

    : base(NormalizePath(path))
  {
  }

  private static string NormalizePath(
    FilePath path)
  {
    const string PROPS_POSTFIX = ".props.json";

    if (path.Value.EndsWith(PROPS_POSTFIX))
    {
      return path.Value;
    }
    return string.Concat(path, PROPS_POSTFIX);
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
