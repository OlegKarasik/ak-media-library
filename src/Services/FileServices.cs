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

  public static async Task Save(
    IndexItem index,
    DirectoryPath path)
  {
    if (index is null)
    {
      throw new ArgumentNullException(nameof(index));
    }

    var content = JsonSerializer.Serialize(
      index,
      options);

    await File.WriteAllTextAsync(
      new IndexPath(path.Value).Value, 
      content);
  }
}
