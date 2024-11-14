namespace MediaLibrary.Commands;

public class ScanCommandOptions
{
  public IReadOnlyCollection<string> VideoExtensions 
  { 
    get; 
  }

  public IReadOnlyCollection<string> IgnoreMatchPatterns 
  { 
    get; 
  }

  public IReadOnlyCollection<string> ShowMatchPatterns 
  { 
    get; 
  }

  public IReadOnlyCollection<string> SeasonMatchPatterns 
  { 
    get; 
  }

  public IReadOnlyCollection<string> MovieMatchPatterns 
  { 
    get; 
  }

  public IReadOnlyCollection<string> EpisodeMatchPatterns 
  { 
    get; 
  }

  public ScanCommandOptions()
  {
    this.VideoExtensions = [".mp4", ".avi", ".mkv"];

    this.IgnoreMatchPatterns = 
      [
        @"^\._"
      ];
    this.ShowMatchPatterns = 
      [
        @"^(?<htitle>.+)$"
      ];
    this.SeasonMatchPatterns = 
      [
        @"^(?<stitle>.+\s*(?<sindex>\d+))$"
      ];
    this.MovieMatchPatterns = 
      [
        @"^(?<mtitle>.+)$"
      ];
    this.EpisodeMatchPatterns =
      [
        @"^S(?<sindex>\d+)E(?<eindexf>\d+)\s*-\s*E(?<eindext>\d+)\s*-?\s*(?<etitle>.+)$",
        @"^S(?<sindex>\d+)E(?<eindex>\d+)\s*-?\s*(?<etitle>.+)$"
      ];
  }
}
