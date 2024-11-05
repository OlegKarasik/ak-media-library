namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public LibraryItemMask Mask
  {
    get;
  }

  public Dictionary<string, MovieItem> Movies
  {
    get;
  }

  public Dictionary<string, ShowItem> Shows
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
    this.Movies = Collide<MovieItem, MovieItemKey>(
      libraries.SelectMany(i => i.Movies.Values));

    this.Shows = Collide<ShowItem, ShowItemKey>(
      libraries.SelectMany(i => i.Shows.Values));

    if (this.Movies.Count != 0)
    {
      this.Mask |= LibraryItemMask.Movies;
    }
    if (this.Shows.Count != 0)
    {
      this.Mask |= LibraryItemMask.Shows;
    }
  }

  public LibraryItem(
    IEnumerable<MovieItem> movies)

    : this()
  {
    this.Mask = LibraryItemMask.Movies;
    this.Movies = Collide<MovieItem, MovieItemKey>(movies);
  }

  public LibraryItem(
    IEnumerable<ShowItem> shows)

    : this()
  {
    this.Mask = LibraryItemMask.Shows;
    this.Shows = Collide<ShowItem, ShowItemKey>(shows);
  }
}
