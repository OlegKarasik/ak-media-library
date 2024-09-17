using System.Text.Json;

using MediaLibrary.Business.Properties;

namespace MediaLibrary.Business;

public static class Props
{
  public static async Task<T> GetAsync<T>(
    PropsPath path)

    where T : new()
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    using var stream = new FileStream(path.Value, FileMode.OpenOrCreate);

    var props = await JsonSerializer.DeserializeAsync<T>(stream);
    return props is not null 
      ? props 
      : new T();
  }

  public static async Task SaveAsync<T>(
    PropsPath path,
    T props)
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }
    if (props is null)
    {
      throw new ArgumentNullException(nameof(props));
    }

    using var stream = new FileStream(path.Value, FileMode.OpenOrCreate);

    await JsonSerializer.SerializeAsync(stream, props);
  }
}
