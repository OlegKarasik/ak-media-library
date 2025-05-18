using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class ShowItem : Item<DirectoryPath>, IComparable<ShowItem>
{
  public required string Title
  {
    get; init;
  }

  public required SeasonItem[] Seasons 
  { 
    get; init;
  }

  public int CompareTo(
    ShowItem? other)
  {
    if (other is null)
    {
      return 1;
    }
    return StringComparer.OrdinalIgnoreCase.Compare(this.Title, other.Title);
  }
}
