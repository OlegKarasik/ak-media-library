using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class MoviesNavigationSegment : NavigationSegment
{
  private readonly IDictionary<string, MovieItem> movies;

  public override NavigationSegment this[string key]
  {
    get
    {
      if (this.movies.TryGetValue(key, out var movie))
      {
        return new ItemNavigationSegment(movie);
      }
      return new NoneNavigationSegment();
    }
  }

  public MoviesNavigationSegment(
    LibraryItem item)

    : base(item)
  {
    if (item is null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    this.movies = item.Movies;
  }
}
