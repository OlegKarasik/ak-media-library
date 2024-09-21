using System.Diagnostics;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
public class DirectoryPath
{
  public string Name 
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

    this.Name = Path.GetFileName(value) ?? value;
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
