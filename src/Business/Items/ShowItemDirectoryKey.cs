namespace MediaLibrary.Business.Items;

public struct ShowItemDirectoryKey : IDirectoryKey<ShowItem>
{
  public readonly string Get(
    ShowItem item)
  {
    return item switch
    {
      null => throw new ArgumentNullException(nameof(item)),
      _ => item.Title
    };
  }
}
