using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}{{{nameof(Extension)},nq}}")]
public class FilePath
{
  [JsonInclude]
  public string Name 
  { 
    get; private set;
  }

  [JsonInclude]
  public string Directory
  {
    get; private set;
  }

  [JsonInclude]
  public string Extension 
  { 
    get; private set; 
  }

  [JsonInclude]
  public string Value 
  { 
    get; private set;
  }

#pragma warning disable CS8618

  [JsonConstructor]
  protected FilePath()
  {
  }

#pragma warning restore CS8618

  public FilePath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.Name = Path.GetFileNameWithoutExtension(value);
    this.Directory = Path.GetDirectoryName(value) ?? Path.GetPathRoot(value) ?? string.Empty;
    this.Extension = Path.GetExtension(value);
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
