using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class EpisodeItem : FileItem
{
  public required string Title
  { 
    get; init;
  }
}
