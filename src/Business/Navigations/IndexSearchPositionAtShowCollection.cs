using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtShowCollection : IndexSearchPosition
{
  private readonly ShowItem[] shows;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      var show = Array.Find(this.shows, i => i.Title == key);
      return show is not null
        ? new IndexSearchPositionAtShow(show)
        : new IndexSearchPositionAtEmpty();
    }
  }

  public IndexSearchPositionAtShowCollection(
    ShowCollectionItem collectionItem)

    : base(collectionItem)
  {
    ArgumentNullException.ThrowIfNull(collectionItem);

    this.shows = collectionItem.Shows;
  }
}
