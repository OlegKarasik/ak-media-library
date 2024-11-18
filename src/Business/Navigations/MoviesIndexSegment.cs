using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class MoviesIndexSegment : IndexSegment
{
  private readonly IDictionary<string, MovieItem> movies;

  public override IndexSegment this[string key]
  {
    get
    {
      if (this.movies.TryGetValue(key, out var movie))
      {
        return new ItemIndexSegment(movie);
      }
      return new NoneIndexSegment();
    }
  }

  public MoviesIndexSegment(
    IndexItem item)

    : base(item)
  {
    if (item is null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    this.movies = item.Movies;
  }
}
