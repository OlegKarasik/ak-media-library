using System.Diagnostics;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}{{{nameof(Extension)},nq}}")]
public class FilePath
{
  public string Name 
  { 
    get; 
  }

  public string Extension 
  { 
    get; 
  }

  public string Value 
  { 
    get; 
  }

  public FilePath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.Name = Path.GetFileNameWithoutExtension(value);
    this.Extension = Path.GetExtension(value);
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
