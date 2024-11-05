namespace MediaLibrary.Business.Items;

public struct EpisodeItemKey : IDirectoryKey<EpisodeItem>
{
  public readonly string Get(
    EpisodeItem item)
  {
    return item switch
    {
      null => throw new ArgumentNullException(nameof(item)),
      _ => item.Title
    };
  }
}