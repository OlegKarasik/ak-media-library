namespace MediaLibrary.Business.Items;

public abstract class FileItem
{
  public required FilePath Path 
  { 
    get; init; 
  }
}
