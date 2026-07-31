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

  protected static async Task<IndexItem> GetAsync(
    FilePathIndex path)
  {
    ArgumentNullException.ThrowIfNull(path);

    using var fs = File.OpenRead(path.Value);
    return await JsonSerializer.DeserializeAsync<IndexItem>(fs, jsonOptions)
      ?? throw new InvalidOperationException($"Unable to load index from {path.Value}");
  }

  protected static async Task<TProps?> GetAsync<TProps>(
    FilePathProps path)

    where TProps: class
  {
    ArgumentNullException.ThrowIfNull(path);

    if (!File.Exists(path.Value))
    {
      return null;
    }

    using var fs = File.OpenRead(path.Value);
    return await JsonSerializer.DeserializeAsync<TProps>(fs, jsonOptions)
      ?? throw new InvalidOperationException($"Unable to load properties file from {path.Value}");
  }

  protected static async Task SaveAsync<TPath>(
    byte[] bytes,
    TPath path)

    where TPath: FilePath
  {
    ArgumentNullException.ThrowIfNull(bytes);
    ArgumentNullException.ThrowIfNull(path);

    await File.WriteAllBytesAsync(path.Value, bytes);
  }

  protected static async Task SaveAsync<T, TPath>(
    T value,
    TPath path)

    where T: class
    where TPath: FilePath
  {
    ArgumentNullException.ThrowIfNull(value);
    ArgumentNullException.ThrowIfNull(path);

    await File.WriteAllTextAsync(
      path.Value, 
      JsonSerializer.Serialize(value, jsonOptions));
  }
}
