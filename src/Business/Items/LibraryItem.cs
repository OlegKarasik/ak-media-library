namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public MovieItem[] Movies 
  { 
    get;
  }

  public ShowItem[] Shows 
  { 
    get;  
  }

  public LibraryItem(
    IEnumerable<LibraryItem> libraries,
    IEnumerable<MovieItem> movies,
    IEnumerable<ShowItem> shows)
  {
    this.Movies = [.. (movies ?? []), .. (libraries ?? []).SelectMany(i => i.Movies)];
    this.Shows = [.. (shows ?? []), .. (libraries ?? []).SelectMany(i => i.Shows)];
  }

  public LibraryItem(
    IEnumerable<MovieItem> movies,
    IEnumerable<ShowItem> shows)

    : this([], movies, shows)
  {
  }

  public LibraryItem(
    IEnumerable<MovieItem> movies)

    : this(movies, [])
  {
  }

  public LibraryItem(
    IEnumerable<ShowItem> shows)

    : this([], shows)
  {
  }
}
