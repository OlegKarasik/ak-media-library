namespace MediaLibrary.Business.Items;

public class SeasonPropsItem
{
  public string? Title
  {
    get; set;
  }

  public string[]? Summary
  {
    get; set;
  }

  public ItemPosition? MemoryPosition
  {
    get; set;
  }
}
