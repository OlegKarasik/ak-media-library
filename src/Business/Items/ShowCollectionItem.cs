using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"Show Collection, Count: {{{nameof(Shows)}.Length}}")]
public class ShowCollectionItem : Item
{
  public required ShowItem[] Shows
  {
    get; init;
  }
}
