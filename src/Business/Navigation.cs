using MediaLibrary.Business.Items;

namespace MediaLibrary.Business;

public enum NavigationRoot
{
  Movies,
  Shows
}

public abstract class NavigationSegment
{
  public abstract NavigationSegment this[string key]
  {
    get;
  }
}

public class ItemNavigationSegment<TItem> : NavigationSegment
  where TItem: Item
{
  private readonly TItem item;

  public ItemNavigationSegment(
    TItem item)
  {
    this.item = item ?? throw new ArgumentNullException(nameof(item));
  }

  public override NavigationSegment this[string key] => this;
}

public class MoviesNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, MovieItem> movies;

  public MoviesNavigationSegment(
    IDictionary<string, MovieItem> movies)
  {
    this.movies = movies ?? throw new ArgumentNullException(nameof(movies));
  }

  public override NavigationSegment this[string key] => new ItemNavigationSegment<MovieItem>(this.movies[key]);
}

public class ShowsNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, ShowItem> shows;

  public ShowsNavigationSegment(
    IDictionary<string, ShowItem> shows)
  {
    this.shows = shows ?? throw new ArgumentNullException(nameof(shows));
  }

  public override NavigationSegment this[string key] => new SeasonNavigationSegment(this.shows[key].Seasons);
}

public class SeasonNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, SeasonItem> seasons;

  public SeasonNavigationSegment(
    IDictionary<string, SeasonItem> seasons)
  {
    this.seasons = seasons ?? throw new ArgumentNullException(nameof(seasons));
  }

  public override NavigationSegment this[string key] => new EpisodeNavigationSegment(this.seasons[key].Episodes);
}

public class EpisodeNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, EpisodeItem> episodes;

  public EpisodeNavigationSegment(
    IDictionary<string, EpisodeItem> episodes)
  {
    this.episodes = episodes ?? throw new ArgumentNullException(nameof(episodes));
  }

  public override NavigationSegment this[string key] => new ItemNavigationSegment<EpisodeItem>(this.episodes[key]);
}

public class NavigationPath
{
  public NavigationRoot Root
  {
    get;
  }
}

public class Navigation
{
  public static Item GetItem(
    LibraryItem library,
    NavigationPath path)
  {
    NavigationSegment segment = path.Root switch
    {
      NavigationRoot.Movies => new MoviesNavigationSegment(library.Movies),
      NavigationRoot.Shows => new ShowsNavigationSegment(library.Shows),
      _ => throw new NotImplementedException()
    };


    return null;
  }
}
