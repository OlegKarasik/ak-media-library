namespace MediaLibrary.Business.Items;

public interface IDirectoryKey<T>
{
  string Get(T item);
}

public abstract class DirectoryItem
{
  public required DirectoryPath Path 
  { 
    get; init;
  }

  protected static Dictionary<string, T> Collide<T, TKey>(
    IEnumerable<T> items)

    where TKey : IDirectoryKey<T>, new()
  {
    var libraryKey = new TKey();

    Dictionary<string, T> result = [];
    Dictionary<string, int> indices = [];
    foreach (var item in items)
    {
      var key = libraryKey.Get(item);
      if (!result.TryAdd(key, item))
      {
        if (!indices.TryGetValue(key, out var index))
        {
          index = 1;
        }

        if (!result.TryAdd($"{key} ({index++})", item))
        {
          throw new Exception();
        }

        indices[key] = index;
      }  
    }
    return result;
  }
}
