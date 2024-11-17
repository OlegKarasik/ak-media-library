using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class SeasonNavigationSegment : NavigationSegment
{
  private readonly SeasonItem season;

  public override NavigationSegment this[string key]
  {
    get
    {
      if (this.season.Episodes.TryGetValue(key, out var episode))
      {
        return new ItemNavigationSegment(episode);
      }
      return new NoneNavigationSegment();
    }
  }

  public SeasonNavigationSegment(
    SeasonItem item)

    : base(item)
  {
    this.season = item ?? throw new ArgumentNullException(nameof(item));
  }
}
