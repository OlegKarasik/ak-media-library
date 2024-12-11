using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtShowCollection : IndexSearchPosition
{
  private readonly Dictionary<string, ShowItem> shows;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      return this.shows.TryGetValue(key, out var show) 
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
