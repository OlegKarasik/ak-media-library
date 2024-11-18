using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class ShowIndexSegment : IndexSegment
{
  private readonly ShowItem show;

  public override IndexSegment this[string key]
  {
    get
    {
      if (this.show.Seasons.TryGetValue(key, out var season))
      {
        return new SeasonIndexSegment(season);
      }
      return new NoneIndexSegment();
    }
  }

  public ShowIndexSegment(
    ShowItem item)

    : base(item)
  {
    this.show = item ?? throw new ArgumentNullException(nameof(item));
  }
}
