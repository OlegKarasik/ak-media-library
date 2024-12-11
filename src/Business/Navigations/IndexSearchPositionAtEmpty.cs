using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtEmpty : IndexSearchPosition
{
  public override IndexSearchPosition this[string key] => new IndexSearchPositionAtEmpty();

  public IndexSearchPositionAtEmpty()
    : base(new NoneItem())
  {
  }
}
