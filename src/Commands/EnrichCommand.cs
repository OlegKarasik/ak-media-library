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
      case ShowItem show:
        {
          var a = await this.enrichment.Search(show.Title, EnrichmentService.SearchTarget.Series);
          foreach (var x in a)
          {
            AnsiConsole.WriteLine(x.Name);
            if (x.Overview is not null)
            {
              AnsiConsole.WriteLine(x.Overview);
            }
            AnsiConsole.WriteLine();
          }
          
          var b = await this.enrichment.GetEpisodeListAsync(a[0].Id);
          var c = b.ToDictionary(i => i.Name, i => i.Id);

          foreach (var episode in show.Seasons.SelectMany(i => i.Value.Episodes))
          {
            if (c.TryGetValue(episode.Value.Title, out var id))
            {
              AnsiConsole.WriteLine($"Matched: {episode.Value.Title}");
            }
            else
            {
              AnsiConsole.WriteLine($"Unmatched: {episode.Value.Title}");
            }
          }
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }
}
