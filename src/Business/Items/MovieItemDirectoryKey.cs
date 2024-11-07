namespace MediaLibrary.Business.Items;

public struct MovieItemDirectoryKey : IDirectoryKey<MovieItem>
{
  public readonly string Get(
    MovieItem item)
  {
    return item switch
    {
      null => throw new ArgumentNullException(nameof(item)),
      _ => item.Title
    };
  }
}