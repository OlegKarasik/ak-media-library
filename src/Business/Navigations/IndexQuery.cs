using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MediaLibrary.Business.Navigation;

[TypeConverter(typeof(TypeConverter))]
public class IndexQuery
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
      return value is string s ? new IndexQuery(s) : null;
    }

    public override object? ConvertTo(
      ITypeDescriptorContext? context, 
      CultureInfo? culture, 
      object? value, 
      Type destinationType)
    {
      return value is IndexQuery query ? query.ToString() : null;
    }
  }

  private const string DELIMITER = "::";

  public IndexQueryRoot Root
  {
    get;
  }

  public IEnumerable<string> Sections
  {
    get;
  }

  public IndexQuery(
    string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
    }

    var values = value
      .Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (values.Length == 0 || values.Length == 1)
    {
      throw new ArgumentException("The navigation query must include a root and path (for instance, 'Shows/Boston Legal')");
    }
    if (!Enum.TryParse<IndexQueryRoot>(values[0], out var root))
    {
      throw new ArgumentException($"The navigation query must start from one of the roots: {string.Join(",", Enum.GetValues<IndexQueryRoot>())}");
    }

    this.Root = root;
    this.Sections = new ArraySegment<string>(values, 1, values.Length - 1);
  }

  public override string ToString()
  {
    return $"{this.Root}{DELIMITER}{string.Join(DELIMITER, this.Sections)}";
  }
}
