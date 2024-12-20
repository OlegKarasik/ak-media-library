using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

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
    IndexItem index = await FileServices.Load(new DirectoryIndexFilePath(settings.Library.Value));

    PanelHeader outputTitle = null;

    FilePath[] suppliments = [];
    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case MovieItem movie:
        {
          outputTitle = new PanelHeader(movie.Title, Justify.Center);
          suppliments = [
            new FilePropsFilePath(movie.Path.Value),
            .. this.options.SubtitleExtensions.Select(extension => new FileSubtitlesFilePath(Path.ChangeExtension(movie.Path.Value, extension)))
          ];
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    var outputContent = new List<IRenderable>();
    foreach (var path in suppliments)
    {
      switch (path)
      {
        case FilePropsFilePath props:
          outputContent.Add(new Markup("Props: TRUE"));
          break;
        case FileSubtitlesFilePath subtitles:
          outputContent.Add(new Markup($"{subtitles.Extension}: TRUE"));
          break;
      }
    }

    AnsiConsole.Write(
      new Panel(new Rows(outputContent)) {
        Header = outputTitle
      });

    return 0;
  }
}
