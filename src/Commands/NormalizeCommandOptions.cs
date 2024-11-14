namespace MediaLibrary.Commands;

public class NormalizeCommandOptions
{
  public IReadOnlyCollection<string> SubtitlesExtensions 
  { 
    get; 
  }

  public NormalizeCommandOptions()
  {
    this.SubtitlesExtensions = [".srt", ".ass"];
  }
}
