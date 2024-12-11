using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtItem : IndexSearchPosition
{
  public override IndexSearchPosition this[string key] => new IndexSearchPositionAtEmpty();

  public IndexSearchPositionAtItem(
    Item item)

    : base(item)
  {
  }
}
