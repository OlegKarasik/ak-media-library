using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class EpisodeItem : FileItem, IComparable<EpisodeItem>
{
  public required string Title
  { 
    get; init;
  }

  public required ItemPosition SeasonPosition
  {
    get; init;
  }

  public required ItemPosition EpisodePosition
  {
    get; init;
  }

  public int CompareTo(
    EpisodeItem? other)
  {
    if (other is null)
    {
      return 1;
    }
    var result = this.SeasonPosition.CompareTo(other.SeasonPosition);
    if (result != 0)
    {
      return result;
    }
    return this.EpisodePosition.CompareTo(other.EpisodePosition);
  }
}
