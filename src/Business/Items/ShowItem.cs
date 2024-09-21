using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class ShowItem : DirectoryItem
{
  public required string Title
  {
    get; init;
  }

  public required SeasonItem[] Seasons 
  { 
    get; init; 
  }
}
