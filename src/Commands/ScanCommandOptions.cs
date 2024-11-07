namespace MediaLibrary.Commands;

public class ScanCommandOptions
{
  public IReadOnlyCollection<string> FileExtensions 
  { 
    get; 
  }

  public IReadOnlyCollection<string> FileIgnorePatterns 
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
    this.FileExtensions = [".mp4", ".avi", ".mkv"];
    this.FileIgnorePatterns = 
      [
        @"^\._"
      ];

    this.ShowMatchPatterns = 
      [
        @"^(?<title>.+)$"
      ];
    this.SeasonMatchPatterns = 
      [
        @"^(?<title>.+)$"
      ];
    this.MovieMatchPatterns = 
      [
        @"^(?<title>.+)$"
      ];
    this.EpisodeMatchPatterns =
      [
        @"^S\d+E(?<from>\d+)\s*-\s*E(?<to>\d+)\s*-?\s*(?<title>.+)$",
        @"^S\d+E(?<index>\d+)\s*-?\s*(?<title>.+)$"
      ];
  }
}
