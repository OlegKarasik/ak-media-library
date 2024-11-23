using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace MediaLibrary.Business;

[TypeConverter(typeof(TypeConverter))]
[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
public class DirectoryPath
{
  public class TypeConverter : System.ComponentModel.TypeConverter
  {
    public override bool CanConvertFrom(
      ITypeDescriptorContext? context, 
      Type sourceType)
    {
      return sourceType is not null && sourceType == typeof(string);
    }
    public override bool CanConvertTo(
      ITypeDescriptorContext? context, 
      [NotNullWhen(true)] Type? destinationType)
    {
      return destinationType is not null && destinationType == typeof(string);
    }

    public override object? ConvertFrom(
      ITypeDescriptorContext? context, 
      CultureInfo? culture, 
      object value)
    {
      return value is string s ? new DirectoryPath(s) : null;
    }

    public override object? ConvertTo(
      ITypeDescriptorContext? context, 
      CultureInfo? culture, 
      object? value, 
      Type destinationType)
    {
      return value is DirectoryPath path ? path.Value : null;
    }
  }
  
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

    this.Name = Path.GetFileName(value) ?? value;
    this.Value = value;
  }

  public override string ToString()
  {
    return this.Value;
  }
}
