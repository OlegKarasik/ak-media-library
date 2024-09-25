using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class SeasonItem : DirectoryItem
{
  public required string Title
  {
    get; init;
  }

  public EpisodeItem[] Episodes 
  { 
    get; 
  }

  public SeasonItem(
    IEnumerable<EpisodeItem> episodes)
  {
    this.Episodes = [.. (episodes ?? [])];
  }
}
