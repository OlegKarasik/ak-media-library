using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class NoneIndexSegment : IndexSegment
{
  public override IndexSegment this[string key] => new NoneIndexSegment();

  public NoneIndexSegment()
    : base(new NoneItem())
  {
  }
}
