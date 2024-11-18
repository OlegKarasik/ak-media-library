using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ItemIndexSegment : IndexSegment
{
  public override IndexSegment this[string key] => new NoneIndexSegment();

  public ItemIndexSegment(
    Item item)

    : base(item)
  {
  }
}
