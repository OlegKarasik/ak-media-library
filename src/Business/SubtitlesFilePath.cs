namespace MediaLibrary.Business;

public class SubtitlesFilePath : FilePath
{
  public SubtitlesFilePath(
    FilePath path)

    : base(path.Value)
  {
  }
}
