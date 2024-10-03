namespace MediaLibrary.Business.Items;
public class LibraryItem : DirectoryItem
{
  public LibraryItemMask Mask
  {
    get;
  }

  public MovieItem[] Movies
  {
    get;
  }

  public ShowItem[] Shows
  {
    get;
  }

  private LibraryItem()
  {
    this.Movies = [];
    this.Shows = [];
  }

  public LibraryItem(
    IEnumerable<LibraryItem> libraries)

    : this()
  {
    this.Movies = [.. SelectMovies(libraries)];
    this.Shows = [.. SelectShows(libraries)];

    if (this.Movies.Length != 0)
    {
      this.Mask |= LibraryItemMask.Movies;
    }
    if (this.Shows.Length != 0)
    {
      this.Mask |= LibraryItemMask.Shows;
    }
  }

  public LibraryItem(
    IEnumerable<MovieItem> movies)

    : this()
  {
    this.Mask = LibraryItemMask.Movies;
    this.Movies = [.. movies ?? []];
  }

  public LibraryItem(
    IEnumerable<ShowItem> shows)

    : this()
  {
    this.Mask = LibraryItemMask.Shows;
    this.Shows = [.. shows ?? []];
  }

  private static IEnumerable<MovieItem> SelectMovies(
    IEnumerable<LibraryItem> libraries)
  {
    return libraries
      .Where(i => i.Mask.HasFlag(LibraryItemMask.Movies))
      .SelectMany(i => i.Movies);
  }

  private static IEnumerable<ShowItem> SelectShows(
    IEnumerable<LibraryItem> libraries)
  {
    return libraries
      .Where(i => i.Mask.HasFlag(LibraryItemMask.Shows))
      .SelectMany(i => i.Shows);
  }
}
