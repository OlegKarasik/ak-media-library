using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

public class DirectoryIndexFilePath : FilePath
{
  [JsonConstructor]
  public DirectoryIndexFilePath(
    string value)

    : base(NormalizePath(value))
  {
  }

  public DirectoryIndexFilePath(
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
