namespace MediaLibrary.Business;

public class DirectoryPropsFilePath : FilePath
{
  public DirectoryPropsFilePath(
    string value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    string value)
  {
    const string PROPS_FILE = "this.props.json";

    if (value.EndsWith(PROPS_FILE))
    {
      return value;
    }
    return Path.Combine(value, PROPS_FILE);
  }
}
