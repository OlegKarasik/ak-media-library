using System.Diagnostics;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

[DebuggerDisplay($"{{{nameof(Current)}}}")]
public abstract class IndexSegment
{
  public Item Current
  {
    get;
  }

  public abstract IndexSegment this[string key]
  {
    get;
  }

  protected IndexSegment(
    Item item)
  {
    this.Current = item ?? throw new ArgumentNullException(nameof(item));
  }
}
