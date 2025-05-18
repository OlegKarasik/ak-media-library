namespace MediaLibrary.Commands;

public class NormaliseCommandOptions
{
  public string EpisodeSplitSymbol
  {
    get;
  }
  
  public NormaliseCommandOptions()
  {
    this.EpisodeSplitSymbol = "&&";
  }
}
