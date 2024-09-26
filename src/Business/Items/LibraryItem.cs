namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public LibraryItemMask Mask
  {
    get;
  }

  public LibraryItem[] Libraries
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

  private LibraryItem(
    LibraryItemMask mask,
    IEnumerable<LibraryItem> libraries,
    IEnumerable<MovieItem> movies,
    IEnumerable<ShowItem> shows)
  {
    this.Mask = mask;
    this.Libraries = [.. libraries ?? []];
    this.Movies = [.. movies ?? []];
    this.Shows = [.. shows ?? []];
  }

  public LibraryItem(
    IEnumerable<LibraryItem> libraries)

    : this(LibraryItemMask.Libraries, libraries, [], [])
  {
  }

  public LibraryItem(
    IEnumerable<MovieItem> movies)

    : this(LibraryItemMask.Movies, [], movies, [])
  {
  }

  public LibraryItem(
    IEnumerable<ShowItem> shows)

    : this(LibraryItemMask.Shows, [], [], shows)
  {
  }
}
