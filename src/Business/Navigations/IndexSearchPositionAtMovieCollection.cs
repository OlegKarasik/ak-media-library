using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtMovieCollection : IndexSearchPosition
{
  private readonly Dictionary<string, MovieItem> movies;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      return this.movies.TryGetValue(key, out var movie) 
        ? new IndexSearchPositionAtItem(movie) 
        : new IndexSearchPositionAtEmpty();
    }
  }

  public IndexSearchPositionAtMovieCollection(
    IndexItem movies)

    : base(movies)
  {
    if (movies is null)
    {
      throw new ArgumentNullException(nameof(movies));
    }

    this.movies = movies.Movies;
  }
}
