using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}{{{nameof(Extension)},nq}}")]
public class FilePath
{
  [JsonIgnore]
  public string Name 
  { 
    get; 
  }

  [JsonIgnore]
  public string Extension 
  { 
    get; 
  }

  public string Value 
  { 
    get; 
  }

  [JsonConstructor]
  public FilePath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.Name = System.IO.Path.GetFileNameWithoutExtension(value);
    this.Extension = System.IO.Path.GetExtension(value);
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
