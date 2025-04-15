using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
public class DirectoryPath
{
  [JsonIgnore]
  public string Name 
  { 
    get; 
  }

  public string Value 
  { 
    get; 
  }

  [JsonConstructor]
  public DirectoryPath(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    this.Name = System.IO.Path.GetFileName(value) ?? value;
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
