namespace MediaLibrary.Commands;

public class EnrichCommandOptions
{
  public string EpisodeSplitSymbol
  {
    get;
  }
  
  public EnrichCommandOptions()
  {
    this.EpisodeSplitSymbol = "&&";
  }
}
