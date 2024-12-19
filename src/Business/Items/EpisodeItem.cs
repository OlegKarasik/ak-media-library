using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class EpisodeItem : Item<FilePath>, IComparable<EpisodeItem>
{
  public required string Title
  { 
    get; init;
  }

  public required ItemPosition Position
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
    return this.Position.CompareTo(other.Position);
  }
}
