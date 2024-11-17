using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"Library, Movies: {{{nameof(Movies)}.Count}}, Shows: {{{nameof(Shows)}.Count}}")]
public class LibraryItem : DirectoryItem
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
