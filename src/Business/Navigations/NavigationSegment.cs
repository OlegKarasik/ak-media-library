using System.Diagnostics;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

[DebuggerDisplay($"{{{nameof(Current)}}}")]
public abstract class NavigationSegment
{
  public Item Current
  {
    get;
  }

  public abstract NavigationSegment this[string key]
  {
    get;
  }

  protected NavigationSegment(
    Item item)
  {
    this.Current = item ?? throw new ArgumentNullException(nameof(item));
  }
}
