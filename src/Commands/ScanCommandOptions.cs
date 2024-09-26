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
        @"^S\d+(?<episode>E\d+\s*-\s*E\d+)\s*-?\s*(?<title>.+)$",
        @"^S\d+(?<episode>E\d+)\s*-?\s*(?<title>.+)$"
      ];
  }
}
