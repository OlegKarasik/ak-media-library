using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"Movie Collection, Count: {{{nameof(Movies)}.Length}}")]
public class MovieCollectionItem : Item
{
  public required MovieItem[] Movies
  {
    get; init;
  }
}
