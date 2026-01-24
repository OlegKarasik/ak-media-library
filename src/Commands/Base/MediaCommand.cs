using System.Text.Encodings.Web;
using System.Text.Json;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands.Base;

public abstract class MediaCommand<TSettings> : AsyncCommand<TSettings>
  where TSettings : CommandSettings
{
  private static readonly JsonSerializerOptions jsonOptions;

  static MediaCommand()
  {
    jsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
  }

  public static async Task<IndexItem> GetAsync(
    FilePathIndex path)
  {
    ArgumentNullException.ThrowIfNull(path);

    using var fs = File.OpenRead(path.Value);
    return await JsonSerializer.DeserializeAsync<IndexItem>(fs, jsonOptions)
      ?? throw new InvalidOperationException($"Unable to load index from {path.Value}");
  }

  public static async Task SaveAsync<T>(
    byte[] bytes,
    T path)

    where T: FilePath
  {
    ArgumentNullException.ThrowIfNull(bytes);
    ArgumentNullException.ThrowIfNull(path);

    await File.WriteAllBytesAsync(path.Value, bytes);
  }

  public static async Task SaveAsync<T, K>(
    T value,
    K path)

    where T: class
    where K: FilePath
  {
    ArgumentNullException.ThrowIfNull(value);
    ArgumentNullException.ThrowIfNull(path);

    await File.WriteAllTextAsync(
      path.Value, 
      JsonSerializer.Serialize(value, jsonOptions));
  }
}
