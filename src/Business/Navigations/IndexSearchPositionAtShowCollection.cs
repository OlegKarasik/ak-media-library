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
    IndexItem shows)

    : base(shows)
  {
    if (shows is null)
    {
      throw new ArgumentNullException(nameof(shows));
    }

    this.shows = shows.Shows;
  }

}
