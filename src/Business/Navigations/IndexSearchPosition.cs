using System.Diagnostics;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

[DebuggerDisplay($"{{{nameof(Current)}}}")]
public abstract class IndexSearchPosition
{
  public Item Current
  {
    get;
  }

  public abstract IndexSearchPosition this[string key]
  {
    get;
  }

  protected IndexSearchPosition(
    Item item)
  {
    this.Current = item ?? throw new ArgumentNullException(nameof(item));
  }
}
