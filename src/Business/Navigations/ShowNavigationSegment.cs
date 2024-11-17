using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ShowNavigationSegment : NavigationSegment
{
  private readonly ShowItem show;

  public override NavigationSegment this[string key]
  {
    get
    {
      if (this.show.Seasons.TryGetValue(key, out var season))
      {
        return new SeasonNavigationSegment(season);
      }
      return new NoneNavigationSegment();
    }
  }

  public ShowNavigationSegment(
    ShowItem item)

    : base(item)
  {
    this.show = item ?? throw new ArgumentNullException(nameof(item));
  }
}
