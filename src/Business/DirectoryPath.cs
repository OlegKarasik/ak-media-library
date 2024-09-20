namespace MediaLibrary.Business;

public class DirectoryPath
{
  public string DirectoryName 
  { 
    get; 
  }

  public string Value 
  { 
    get; 
  }

  public DirectoryPath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.DirectoryName = Path.GetDirectoryName(value) ?? value;
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
