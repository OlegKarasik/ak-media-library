using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"Index, Movies: {{{nameof(Movies)}.Length}}, Shows: {{{nameof(Shows)}.Length}}")]
public class IndexItem : Item<FilePathIndex>
{
  public required MovieItem[] Movies
  {
    get; init;
  }

  public required ShowItem[] Shows
  {
    get; init;
  }
}