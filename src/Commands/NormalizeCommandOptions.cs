namespace MediaLibrary.Commands;

public class NormalizeCommandOptions
{
  public IReadOnlyCollection<string> RelatedExtensions 
  { 
    get; 
  }

  public string EpisodePattern 
  { 
    get; 
  }

  public string EpisodeRangePattern 
  { 
    get; 
  }

  public NormalizeCommandOptions()
  {
    this.RelatedExtensions = [".srt", ".ass", ".props.json"];

    this.EpisodePattern = "S{Season}E{Episode} - <EpisodeTitle>";
    this.EpisodeRangePattern = "S{Season}E{EpisodeOpen} - E{EpisodeClose} - {EpisodeTitle}";
  }
}
