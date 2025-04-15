namespace MediaLibrary.Business;

public class FilePathProps : FilePath
{
  public FilePathProps(
    FilePath path)

    : base(NormalizePath(path))
  {
  }

  public FilePathProps(
    DirectoryPath path)

    : base(NormalizePath(path))
  {
  }

  private static string NormalizePath(
    FilePath path)
  {
    const string PROPS_POSTFIX = ".props.json";

    var value = path.Value;

    if (value.EndsWith(PROPS_POSTFIX))
    {
      return value;
    }
    return string.Concat(value, PROPS_POSTFIX);
  }

  private static string NormalizePath(
    DirectoryPath path)
  {
    const string PROPS_FILE = "this.props.json";

    var value = path.Value;

    if (value.EndsWith(PROPS_FILE))
    {
      return value;
    }
    return Path.Combine(value, PROPS_FILE);
  }
}
