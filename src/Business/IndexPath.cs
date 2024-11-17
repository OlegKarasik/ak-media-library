namespace MediaLibrary.Business;

public class IndexPath : FilePath
{
  public IndexPath(
    string value)

    : base(NormalizePath(value))
  {
  }

  private static string NormalizePath(
    string value)
  {
    const string INDEX_FILE = "this.index.json";

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }
    if (value.EndsWith(INDEX_FILE))
    {
      return value;
    }
    return Path.Combine(value, INDEX_FILE);
  }
}
