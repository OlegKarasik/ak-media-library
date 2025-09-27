using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
public class DirectoryPath
{
  [JsonInclude]
  public string Name
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
  protected DirectoryPath()
  {
  }

#pragma warning restore CS8618

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

  public DirectoryPath WithName(string name)
  {
    ArgumentException.ThrowIfNullOrEmpty(name);

    var parent = Path.GetDirectoryName(this.Value);
    if (parent is null)
    {
      throw new InvalidOperationException("The path is already rooted, and therefore can't be renamed");
    }
    
    return new DirectoryPath(Path.Combine(parent, name));
  }
}
