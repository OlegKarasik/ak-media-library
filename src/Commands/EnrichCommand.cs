using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions.Services;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class EnrichCommand : AsyncCommand<EnrichCommandSettings>
{
  private readonly EnrichCommandOptions options;

  public EnrichCommand()
  {
    this.options = new EnrichCommandOptions();
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    EnrichCommandSettings settings)
  {
    IndexItem index = await FileServices.Load(new DirectoryIndexFilePath(settings.Library.Value));

    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case MovieItem movie:
        {
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }
}
