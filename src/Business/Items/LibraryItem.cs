namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public required Dictionary<string, MovieItem> Movies
  {
    get; init;
  }

  public required Dictionary<string, ShowItem> Shows
  {
    get; init;
  }
  
  public required DirectoryPath Path 
  { 
    get; init;
  }
}
