using System.Text.Json;

using MediaLibrary.Business.Properties;

namespace MediaLibrary.Business;

public static class Props
{
  public static async Task<T> GetAsync<T>(
    PropsPath path)

    where T : PropsJson, new()
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    using var stream = new FileStream((string)path, FileMode.OpenOrCreate);

    var props = await JsonSerializer.DeserializeAsync<T>(stream);
    return props is not null 
      ? props 
      : new T();
  }

  public static async Task SaveAsync<T>(
    PropsPath path,
    T props)

    where T : PropsJson
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }
    if (props is null)
    {
      throw new ArgumentNullException(nameof(props));
    }

    using var stream = new FileStream((string)path, FileMode.OpenOrCreate);

    await JsonSerializer.SerializeAsync(stream, props);
  }
}
