namespace MediaLibrary.Business.Items;

public abstract class FileItem : Item
{
  public required FilePath Path 
  { 
    get; init; 
  }
}
