namespace MediaLibrary.Business.Items;

public class EpisodePropsItem
{
  public string? Title
  {
    get; set;
  }

  public string[]? Summary
  {
    get; set;
  }

  public string? Date
  {
    get; set;
  }

  public string[]? Directors
  {
    get; set;
  }

  public string[]? Writers
  {
    get; set;
  }

  public EpisodeTitle? MemoryTitle 
  { 
    get; set; 
  }
}
