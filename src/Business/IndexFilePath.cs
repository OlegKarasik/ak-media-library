namespace MediaLibrary.Business;

public class IndexFilePath : FilePath
{
  public IndexFilePath(
    DirectoryPath path)

    : base(NormalizePath(path))
  {
  }

  private static string NormalizePath(
    DirectoryPath path)
  {
    const string INDEX_FILE = "this.index.json";

    if (path.Value.EndsWith(INDEX_FILE))
    {
      return path.Value;
    }
    return Path.Combine(path.Value, INDEX_FILE);
  }
}
