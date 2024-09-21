using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class SeasonItem : DirectoryItem
{
  public required string Title
  {
    get; init;
  }

  public required EpisodeItem[] Episodes 
  { 
    get; init; 
  }
}
