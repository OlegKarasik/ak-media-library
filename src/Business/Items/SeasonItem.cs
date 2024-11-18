using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class SeasonItem : DirectoryItem, IComparable<SeasonItem>
{
  public required string Title
  {
    get; init;
  }

  public required ItemPosition SeasonPosition
  {
    get; init;
  }

  public required Dictionary<string, EpisodeItem> Episodes 
  { 
    get; init;
  }

  public int CompareTo(
    SeasonItem? other)
  {
    if (other is null)
    {
      return 1;
    }
    return this.SeasonPosition.CompareTo(other.SeasonPosition);
  }
}
