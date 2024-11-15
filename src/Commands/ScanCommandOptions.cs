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
        @"^(?<ShowTitle>.+)$"
      ];
    this.SeasonMatchPatterns = 
      [
        @"^(?<SeasonTitle>.+\s*(?<Season>\d+))$"
      ];
    this.MovieMatchPatterns = 
      [
        @"^(?<MovieTitle>.+)$"
      ];
    this.EpisodeMatchPatterns =
      [
        @"^S(?<Season>\d+)E(?<EpisodeOpen>\d+)\s*-\s*E(?<EpisodeClose>\d+)\s*-?\s*(?<EpisodeTitle>.+)$",
        @"^S(?<Season>\d+)E(?<Episode>\d+)\s*-?\s*(?<EpisodeTitle>.+)$"
      ];
  }
}
