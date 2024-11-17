using System.Diagnostics;

namespace MediaLibrary.Business.Navigation;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public class NavigationQuery
{
  private const string DELIMITER = "::";

  private string DebuggerDisplay
  {
    get
    {
      return $"{this.Root}{DELIMITER}{string.Join(DELIMITER, this.Sections)}";
    }
  }

  public NavigationQueryRoot Root
  {
    get;
  }

  public IEnumerable<string> Sections
  {
    get;
  }

  public NavigationQuery(
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
      throw new Exception();
    }
    if (!Enum.TryParse<NavigationQueryRoot>(values[0], out var root))
    {
      throw new ArgumentException();
    }

    this.Root = root;
    this.Sections = new ArraySegment<string>(values, 1, values.Length - 1);
  }
}
