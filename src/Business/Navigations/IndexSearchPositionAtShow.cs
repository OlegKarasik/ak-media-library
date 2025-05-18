using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtShow : IndexSearchPosition
{
  private readonly ShowItem show;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      var season = Array.Find(this.show.Seasons, i => i.Title == key);
      return season is not null
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
