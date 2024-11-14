namespace MediaLibrary.Business.Items;

public abstract class DirectoryItem
{
  public required DirectoryPath Path 
  { 
    get; init;
  }
}
