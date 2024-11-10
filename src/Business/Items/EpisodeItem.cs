using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class EpisodeItem : FileItem, IComparable<EpisodeItem>
{
  public required string Title
  { 
    get; init;
  }

  public required ItemIndex SeasonIndex
  {
    get; init;
  }

  public required ItemIndex EpisodeIndex
  {
    get; init;
  }
  
  public required FilePath Path 
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
    var result = this.SeasonIndex.CompareTo(other.SeasonIndex);
    if (result != 0)
    {
      return result;
    }
    return this.EpisodeIndex.CompareTo(other.EpisodeIndex);
  }
}
