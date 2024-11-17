using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class NoneNavigationSegment : NavigationSegment
{
  public override NavigationSegment this[string key] => new NoneNavigationSegment();

  public NoneNavigationSegment()
    : base(new NoneItem())
  {
  }
}
