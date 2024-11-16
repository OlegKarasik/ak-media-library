using MediaLibrary.Business.Items;

namespace MediaLibrary.Business;

public enum NavigationRoot
{
  Movies,
  Shows
}

public abstract class NavigationSegment
{
  public abstract Item this[string key]
  {
    get;
  }
}

public class MoviesNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, MovieItem> movies;

  public MoviesNavigationSegment(
    IDictionary<string, MovieItem> movies)
  {
    this.movies = movies ?? throw new ArgumentNullException(nameof(movies));
  }

  public override Item this[string key] => this.movies[key];
}

public class ShowsNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, ShowItem> shows;

  public ShowsNavigationSegment(
    IDictionary<string, ShowItem> shows)
  {
    this.shows = shows ?? throw new ArgumentNullException(nameof(shows));
  }

  public override Item this[string key] => this.shows[key];
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
