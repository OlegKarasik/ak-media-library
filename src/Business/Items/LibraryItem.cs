namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public required Dictionary<string, List<MovieItem>> Movies 
  { 
    get; init; 
  }

  public required Dictionary<string, List<ShowItem>> Shows 
  { 
    get; init; 
  }
}
