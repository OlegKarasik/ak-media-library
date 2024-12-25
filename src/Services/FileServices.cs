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
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
  }

  public static async Task<IndexItem> LoadAsync(
    DirectoryIndexFilePath path)
  {
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    using var fs = File.OpenRead(path.Value);
    return await JsonSerializer.DeserializeAsync<IndexItem>(fs, options)
      ?? throw new InvalidOperationException($"Unable to load index from {path.Value}");
  }

  public static async Task SaveAsync(
    IndexItem index,
    DirectoryPath path)
  {
    if (index is null)
    {
      throw new ArgumentNullException(nameof(index));
    }

    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    var content = JsonSerializer.Serialize(
      index,
      options);

    await File.WriteAllTextAsync(
      new DirectoryIndexFilePath(path.Value).Value, 
      content);
  }
}
