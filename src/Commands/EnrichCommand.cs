using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions;
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
      case ShowItem show:
        {
          var shows = await this.enrichment.SearchShowAsync(show.Title, EnrichmentService.SearchTarget.Series);

          EnrichmentService.SearchData current;
          for (;;)
          {
            var prompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                  .Title("Please select potential show match:")
                  .AddChoices(shows.Select(i => i.Name)));
            
            var selection = shows.First(i => i.Name == prompt);
            if (selection.Overview is not null)
            {
              AnsiConsole.WriteLine(selection.Overview);
            }

            var confirmation = AnsiConsole.Prompt(
              new ConfirmationPrompt($"Enrich show using data from '{selection.Name}'?"));

            if (confirmation)
            {
              current = selection;
              break;
            }
          };
          
          var episodes = await this.enrichment.ListShowEpisodesAsync(current.Id);
          var c = episodes.ToDictionary(i => i.Name, i => i.Id);

          foreach (var episode in show.Seasons.SelectMany(i => i.Value.Episodes))
          {
            if (c.TryGetValue(episode.Value.Title, out var id))
            {
              AnsiConsole.WriteLine($"Matched: {episode.Value.Title}");
            }
            else
            {
              AnsiConsole.WriteLine($"Unmatched: {episode.Value.Title}");
              foreach (var k in c.Keys)
              {
                if (k.CalculateLevenshteinDistance(episode.Value.Title) < 2)
                {
                  AnsiConsole.WriteLine($"Possible match: {k}");
                }
              }
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
