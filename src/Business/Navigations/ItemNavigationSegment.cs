using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ItemNavigationSegment : NavigationSegment
{
  public override NavigationSegment this[string key] => new NoneNavigationSegment();

  public ItemNavigationSegment(
    Item item)

    : base(item)
  {
  }
}
