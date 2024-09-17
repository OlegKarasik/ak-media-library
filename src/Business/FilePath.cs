namespace MediaLibrary.Business;

public class FilePath
{
  public string FileName { get; }

  public string Value { get; }

  public FilePath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.FileName = Path.GetFileNameWithoutExtension(value);
    this.Value = value;
  }
}
