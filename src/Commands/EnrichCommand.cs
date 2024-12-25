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
    var index = await FileServices.LoadAsync(new DirectoryIndexFilePath(settings.Library.Value));
    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case ShowItem show:
        {
          var id = await this.MatchAsync(show.Title, EnrichmentService.SearchTarget.Series);    
          if (id < 0)
          {
            return 1;
          }      
          var episodes = await this.enrichment.ListEpisodesAsync(id);
          var c = episodes.ToDictionary(i => i.Name, i => i.Id);

          foreach (var episode in show.Seasons.SelectMany(i => i.Value.Episodes))
          {
            if (c.TryGetValue(episode.Value.Title, out var xxx))
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

  private async Task<long> MatchAsync(
    string title,
    EnrichmentService.SearchTarget target)
  {
    const string CANCEL_CHOICE = "[Yellow]Cancel[/]";

    const int OFFSET = 0;
    const int LIMIT  = 20;
    for (;;)
    {
      var results = await AnsiConsole
        .Status()
        .StartAsync("Searching", async ctx => await this.enrichment.SearchAsync(title, target, OFFSET, LIMIT));

      for (;;)
      {
        var prompt = new SelectionPrompt<string>()
          .Title("Select [Blue]an item[/] to view [Green]more[/]:")
          .AddChoiceGroup("Matches", results.Select(i => i.Name));

        prompt.AddChoice(CANCEL_CHOICE);

        var value = AnsiConsole.Prompt(prompt);
        switch (value)
        {
          case CANCEL_CHOICE:
            return -1;
          default:
            {
              var match = results.First(i => i.Name == value);

              AnsiConsole.Write(new Markup($"[Green]{match.Name}[/]"));
              AnsiConsole.WriteLine();
              if (match.Overview is not null)
              {
                AnsiConsole.Write(match.Overview);
                AnsiConsole.WriteLine();
              }

              var confirmation = AnsiConsole.Prompt(
                new ConfirmationPrompt($"Perform enriment using [Blue]{match.Name}[/]?"));

              if (confirmation)
              {
                return match.Id;
              }
              continue;
            }
        }
      }
    };
  }
}
