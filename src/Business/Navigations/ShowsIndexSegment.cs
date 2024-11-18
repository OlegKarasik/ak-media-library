using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ShowsIndexSegment : IndexSegment
{
  private readonly IDictionary<string, ShowItem> shows;

  public override IndexSegment this[string key]
  {
    get
    {
      if (this.shows.TryGetValue(key, out var show))
      {
        return new ShowIndexSegment(show);
      }
      return new NoneIndexSegment();
    }
  }

  public ShowsIndexSegment(
    IndexItem item)

    : base(item)
  {
    if (item is null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    this.shows = item.Shows;
  }

}
