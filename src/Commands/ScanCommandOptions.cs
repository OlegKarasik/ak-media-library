namespace MediaLibrary.Commands;

public class ScanCommandOptions
{
  public IReadOnlyCollection<string> FileExtensions { get; }

  public IReadOnlyCollection<string> FileIgnorePatterns { get; }

  public IReadOnlyCollection<string> MovieMatchPatterns { get; }

  public IReadOnlyCollection<string> EpisodeMatchPatterns { get; }

  public ScanCommandOptions()
  {
    this.FileExtensions = [".mp4", ".avi", ".mkv"];
    this.FileIgnorePatterns = 
      [
        @"^\._"
      ];

    this.MovieMatchPatterns = 
      [
        @".+"
      ];
    this.EpisodeMatchPatterns =
      [
        @"^S\d+E\d+\s+-?\s+.+$",
        @"^S\d+E\d+-E\d+\s+-?\s+.+$"
      ];
  }
}
