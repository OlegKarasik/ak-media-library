namespace MediaLibrary.Commands;

public class InfoCommandOptions
{
  public IReadOnlyCollection<string> SubtitleExtensions 
  { 
    get; 
  }

  public InfoCommandOptions()
  {
    this.SubtitleExtensions = [".ass", ".srt"];
  }
}
