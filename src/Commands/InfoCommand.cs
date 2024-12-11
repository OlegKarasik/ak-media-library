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
    switch (IndexSearch.GetItem(index, settings.IndexQuery)) 
    {
      case MovieItem movie:
        AnsiConsole.WriteLine("Found");
        break;
      default:
        throw new InvalidOperationException($"The '{settings.IndexQuery}' isn't found in index");
    }

    return 0;
  }
}
