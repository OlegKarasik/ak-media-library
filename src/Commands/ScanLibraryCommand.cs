using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business.Properties;
using MediaLibrary.Business;

namespace MediaLibrary.Commands;

public class ScanLibraryCommand : AsyncCommand<ScanLibraryCommandSettings>
{
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
          var LibraryPropsPath = new   PropsPath(settings.LibraryPath);
          var LibraryProps     = await Props.GetAsync<LibraryProps>(LibraryPropsPath);

          AnsiConsole.MarkupLine("Enumerating directories...");
          foreach (var i in Directory.EnumerateDirectories(settings.LibraryPath))
          {
            AnsiConsole.MarkupLine($"- {Path.GetRelativePath(settings.LibraryPath, i)}");
          }

          await Props.SaveAsync<LibraryProps>(LibraryPropsPath, LibraryProps);
        });

    return 0;
  }
}