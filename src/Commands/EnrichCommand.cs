using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class EnrichCommand : AsyncCommand<EnrichCommandSettings>
{
  private readonly EnrichCommandOptions options;
  private readonly EnrichmentService enrichment;

  public EnrichCommand(
    EnrichmentService enrichment)
  {
    this.options = new EnrichCommandOptions();
    this.enrichment = enrichment ?? throw new ArgumentNullException(nameof(enrichment));
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
          var a = await this.enrichment.Search(movie.Title, EnrichmentService.SearchTarget.Movie);
          foreach (var x in a)
          {
            AnsiConsole.WriteLine(x.Name);
            if (x.Overview is not null)
            {
              AnsiConsole.WriteLine(x.Overview);
            }
            AnsiConsole.WriteLine();
          }
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }
}
