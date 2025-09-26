using System.Text.Encodings.Web;
using System.Text.Json;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Extensions.Services;

public static class FileServices
{
  private static readonly JsonSerializerOptions options;

  static FileServices()
  {
    options = new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
  }

  public static async Task<IndexItem> LoadAsync(
    FilePathIndex path)
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    using var fs = File.OpenRead(path.Value);
    return await JsonSerializer.DeserializeAsync<IndexItem>(fs, options)
      ?? throw new InvalidOperationException($"Unable to load index from {path.Value}");
  }

  public static async Task SaveAsync<T>(
    byte[] bytes,
    T path)

    where T: FilePath
  {
    if (bytes is null)
    {
      throw new ArgumentNullException(nameof(bytes));
    }
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    await File.WriteAllBytesAsync(path.Value, bytes);
  }

  public static async Task SaveAsync<T, K>(
    T value,
    K path)

    where T: class
    where K: FilePath
  {
    if (value is null)
    {
      throw new ArgumentNullException(nameof(value));
    }
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    await File.WriteAllTextAsync(
      path.Value, JsonSerializer.Serialize(value, options));
  }
}
