namespace MediaLibrary.Business;

public class FilePathDirectoryImageBackground : FilePath
{
  public FilePathDirectoryImageBackground(
    string value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    string value)
  {
    const string IMAGE_FILE = "this.image-background.jpg";

    if (value.EndsWith(IMAGE_FILE))
    {
      return value;
    }
    return Path.Combine(value, IMAGE_FILE);
  }
}
