using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

public class FilePathImage : FilePath
{
  [JsonConstructor]
  protected FilePathImage()
  {
  }

  public FilePathImage(
    DirectoryPath path)

    : base(NormalizePath(path))
  {
  }

  private static string NormalizePath(
    DirectoryPath path)
  {
    const string IMAGE_FILE = "this.image.jpg";

    var value = path.Value;

    if (value.EndsWith(IMAGE_FILE))
    {
      return value;
    }
    return Path.Combine(value, IMAGE_FILE);
  }
}
