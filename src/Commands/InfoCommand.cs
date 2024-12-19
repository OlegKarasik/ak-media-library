using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;
public partial class InfoCommand : AsyncCommand<InfoCommandSettings>
{
  private readonly InfoCommandOptions options;

  public InfoCommand()
  {
    this.options = new InfoCommandOptions();
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    InfoCommandSettings settings)
  {
    IndexItem index = await FileServices.Load(new IndexFilePath(settings.Library));

    FilePath[] suppliments = [];
    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case MovieItem movie:
        {
          AnsiConsole.WriteLine($"{movie.Path}");

          suppliments = [
            new PropsFilePath(movie.Path),
            .. this.options.SubtitleExtensions.Select(extension => new SubtitlesFilePath(new FilePath(Path.ChangeExtension(movie.Path.Value, extension))))
          ];
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }
    foreach (var path in suppliments)
    {
      switch (path)
      {
        case PropsFilePath props:
          AnsiConsole.WriteLine("Props: TRUE");
          break;
        case SubtitlesFilePath subtitles:
          AnsiConsole.WriteLine($"{subtitles.Extension}: TRUE");
          break;
      }
    }

    return 0;
  }
}
