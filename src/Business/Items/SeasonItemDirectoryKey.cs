namespace MediaLibrary.Business.Items;

public struct SeasonItemDirectoryKey : IDirectoryKey<SeasonItem>
{
  public readonly string Get(
    SeasonItem item)
  {
    return item switch
    {
      null => throw new ArgumentNullException(nameof(item)),
      _ => item.Title
    };
  }
}
