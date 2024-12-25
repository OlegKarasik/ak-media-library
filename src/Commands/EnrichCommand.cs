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
          var remoteId = await this.MatchAsync(show.Title, EnrichmentService.SearchTarget.Series);    
          if (remoteId < 0)
          {
            return 1;
          }      
          await this.MatchEpisodesAsync(show, remoteId);
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

  private async Task<Dictionary<long, EpisodeItem>> MatchEpisodesAsync(
    ShowItem show,
    long remoteId)
  {
    const string SKIP_CHOICE = "[Yellow]Skip[/]";
    
    const int DISTANCE_CONSTANT = 5;

    var output = new Dictionary<long, EpisodeItem>();

    var episodes = show.Seasons
      .SelectMany(i => i.Value.Episodes)
      .ToDictionary(i => i.Key, i => i.Value);

    foreach (var item in await this.enrichment.ListEpisodesAsync(remoteId))
    {
      if (episodes.TryGetValue(item.Name, out var episode))
      {
        output[item.Id] = episode;
      }
      else
      {
        var matches = new List<EpisodeItem>();
        foreach (var (title, value) in episodes)
        {
          if (title.CalculateLevenshteinDistance(item.Name) < DISTANCE_CONSTANT)
          {
            matches.Add(value);
          }
        }
        if (matches.Count == 0)
        {
          AnsiConsole.WriteLine($"Unmatched remote: {item.Name}");
          continue;
        }

        var prompt = new SelectionPrompt<string>()
          .Title("No direct match found, please select one of the [Blue]potential matches[/]:")
          .AddChoiceGroup("Matches", matches.Select(i => i.Title));
        
        prompt.AddChoice(SKIP_CHOICE);

        var choice = AnsiConsole.Prompt(prompt);
        switch (choice)
        {
          case SKIP_CHOICE:
            AnsiConsole.WriteLine($"Skipping remote: {item.Name}");
            break;
          default:
            output[item.Id] = matches.First(i => i.Title == choice);
            break;
        }
      }
    }
    return output;
  }
}
