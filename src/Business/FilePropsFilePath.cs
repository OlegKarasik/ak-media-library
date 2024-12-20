namespace MediaLibrary.Business;

public class FilePropsFilePath : FilePath
{
  public FilePropsFilePath(
    string value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    string value)
  {
    const string PROPS_POSTFIX = ".props.json";

    if (value.EndsWith(PROPS_POSTFIX))
    {
      return value;
    }
    return string.Concat(value, PROPS_POSTFIX);
  }
}
