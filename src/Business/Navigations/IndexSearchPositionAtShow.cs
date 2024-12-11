using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtShow : IndexSearchPosition
{
  private readonly ShowItem show;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      return this.show.Seasons.TryGetValue(key, out var season) 
        ? new IndexSearchPositionAtSeason(season) 
        : new IndexSearchPositionAtEmpty();
    }
  }

  public IndexSearchPositionAtShow(
    ShowItem show)

    : base(show)
  {
    this.show = show ?? throw new ArgumentNullException(nameof(show));
  }
}
