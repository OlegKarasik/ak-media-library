namespace MediaLibrary.Business.Items;

public abstract class DirectoryItem : Item
{
  public required DirectoryPath Path 
  { 
    get; init;
  }
}
