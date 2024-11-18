using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class SeasonIndexSegment : IndexSegment
{
  private readonly SeasonItem season;

  public override IndexSegment this[string key]
  {
    get
    {
      if (this.season.Episodes.TryGetValue(key, out var episode))
      {
        return new ItemIndexSegment(episode);
      }
      return new NoneIndexSegment();
    }
  }

  public SeasonIndexSegment(
    SeasonItem item)

    : base(item)
  {
    this.season = item ?? throw new ArgumentNullException(nameof(item));
  }
}
