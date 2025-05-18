using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearchPositionAtMovieCollection : IndexSearchPosition
{
  private readonly MovieItem[] movies;

  public override IndexSearchPosition this[string key]
  {
    get
    {
      var movie = Array.Find(this.movies, i => i.Title.ToString() == key);
      return movie is not null
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
