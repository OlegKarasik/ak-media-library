using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ShowsNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, ShowItem> shows;

  public override NavigationSegment this[string key]
  {
    get
    {
      if (this.shows.TryGetValue(key, out var show))
      {
        return new ShowNavigationSegment(show);
      }
      return new NoneNavigationSegment();
    }
  }

  public ShowsNavigationSegment(
    LibraryItem item)

    : base(item)
  {
    if (item is null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    this.shows = item.Shows;
  }

}
