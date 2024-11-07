using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class ShowItem : DirectoryItem
{
  public required string Title
  {
    get; init;
  }

  public Dictionary<string, SeasonItem> Seasons 
  { 
    get;
  }

  public ShowItem(
    IEnumerable<SeasonItem> seasons)
  {
    this.Seasons = Collide<SeasonItem, SeasonItemDirectoryKey>(seasons);
  }
}
