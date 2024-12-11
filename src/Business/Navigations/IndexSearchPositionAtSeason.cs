using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtSeason : IndexSearchPosition
{
  private readonly SeasonItem season;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      return this.season.Episodes.TryGetValue(key, out var episode) 
        ? new IndexSearchPositionAtItem(episode) 
        : new IndexSearchPositionAtEmpty();
    }
  }

  public IndexSearchPositionAtSeason(
    SeasonItem season)

    : base(season)
  {
    this.season = season ?? throw new ArgumentNullException(nameof(season));
  }
}
