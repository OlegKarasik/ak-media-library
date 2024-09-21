namespace MediaLibrary.Business.Items;

public class LibraryItem : DirectoryItem
{
  public required MovieItem[] Movies 
  { 
    get; init; 
  }

  public required ShowItem[] Shows 
  { 
    get; init; 
  }
}
