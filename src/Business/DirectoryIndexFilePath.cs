namespace MediaLibrary.Business;

public class DirectoryIndexFilePath : FilePath
{
  public DirectoryIndexFilePath(
    string value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    string value)
  {
    const string INDEX_FILE = "this.index.json";

    if (value.EndsWith(INDEX_FILE))
    {
      return value;
    }
    return Path.Combine(value, INDEX_FILE);
  }
}
