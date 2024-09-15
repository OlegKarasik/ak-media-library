using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

[JsonDerivedType(typeof(LibraryIndexProps), "library-index")]
public class PropsJson
{
}

public class LibraryIndexProps : PropsJson
{
}

public class ScanLibraryCommand : AsyncCommand<ScanLibraryCommandSettings>
{
  private const string INDEX_FILE_NAME = "this.props.json";

  private static async Task<T> GetPropsAsync<T>(
    string path)

    where T : PropsJson, new()
  {
    var file = new FileInfo(path);
    if (file.Exists)
    {
      using var stream = new FileStream(path, FileMode.Open);

      var props = await JsonSerializer.DeserializeAsync<T>(stream);
      if (props is not null)
      {
        AnsiConsole.MarkupLine("Found library index");

        return props;
      }
    }

    AnsiConsole.MarkupLine("No library index found. Creating a new one.");

    return new T();
  }

  private static async Task SavePropsAsync<T>(
    string path,
    T props)

    where T : PropsJson
  {
    using var stream = new FileStream(path, FileMode.OpenOrCreate);

    await JsonSerializer.SerializeAsync(stream, props);
  }

  private static async Task<LibraryIndexProps> GetLibraryIndexAsync(
    string path)
  {
    return await GetPropsAsync<LibraryIndexProps>(Path.Combine(path, INDEX_FILE_NAME));
  }

  private static async Task SaveLibraryIndexAsync(
    string path,
    LibraryIndexProps props)
  {
    await SavePropsAsync<LibraryIndexProps>(Path.Combine(path, INDEX_FILE_NAME), props);
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    ScanLibraryCommandSettings settings)
  {
    await AnsiConsole
      .Status()
      .StartAsync(
        "Scanning...", 
        async ctx => 
        {
          var LibraryIndex = await GetLibraryIndexAsync(settings.LibraryPath);
          await SaveLibraryIndexAsync(settings.LibraryPath, LibraryIndex);
        });

    return 0;
  }
}