using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

public class FilePathIndex : FilePath
{
  [JsonConstructor]
  protected FilePathIndex()
  {
  }

  public FilePathIndex(
    DirectoryPath path)

    : base(NormalizePath(path.Value))
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
