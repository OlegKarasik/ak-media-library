using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"Index, Movies: {{{nameof(Movies)}.Count}}, Shows: {{{nameof(Shows)}.Count}}")]
public class IndexItem : DirectoryItem
{
  public required Dictionary<string, MovieItem> Movies
  {
    get; init;
  }

  public required Dictionary<string, ShowItem> Shows
  {
    get; init;
  }
}
