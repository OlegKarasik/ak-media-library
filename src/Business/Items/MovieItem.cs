using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class MovieItem : Item<FilePath>, IComparable<MovieItem>
{
  public required string Title
  {
    get; init;
  }

  public int CompareTo(
    MovieItem? other)
  {
    if (other is null)
    {
      return 1;
    }
    return StringComparer.OrdinalIgnoreCase.Compare(this.Title, other.Title);
  }
}
