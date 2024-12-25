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
          await this.EnrichAsync(
            show, 
            await this.GetRemoteSeriesAsync(
              await this.GetRemoteIdAsync(show.Title, EnrichmentService.SearchTarget.Series)));
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }

  private async Task<long> GetRemoteIdAsync(
    string title,
    EnrichmentService.SearchTarget target)
  {
    const int OFFSET = 0;
    const int LIMIT  = 20;
    for (;;)
    {
      var results = await AnsiConsole
        .Status()
        .StartAsync("Searching", async ctx => await this.enrichment.SearchAsync(title, target, OFFSET, LIMIT));

      for (;;)
      {
        var value = AnsiConsole.Prompt(
          new SelectionPrompt<string>()
          .Title("Pick the best [Blue]match[/]:")
          .AddChoices(results.Select(i => i.Name)));

        var match = results.First(i => i.Name == value);

        AnsiConsole.Write(new Markup($"[Green]{match.Name}[/]"));
        AnsiConsole.WriteLine();

        if (match.Overview is not null)
        {
          AnsiConsole.Write(match.Overview);
          AnsiConsole.WriteLine();
        }

        if (ConsoleServices.YesNoConfirmation("Accept match?"))
        {
          return match.Id;
        }
      }
    };
  }

  private async Task<EnrichmentService.Series> GetRemoteSeriesAsync(
    long remoteId)
  {
    return await AnsiConsole
      .Status()
      .StartAsync("Getting series", async ctx => (await this.enrichment.GetSeriesAsync(remoteId))!);
  }

  private async Task EnrichAsync(
    ShowItem show,
    EnrichmentService.Series series)
  {
    const string SKIP_CHOICE = "[Yellow]Skip[/]";
    
    const int DISTANCE_CONSTANT = 5;

    var episodes = show.Seasons
      .SelectMany(i => i.Value.Episodes)
      .ToDictionary(i => i.Key, i => i.Value);

    foreach (var item in (series?.Episodes ?? []).Where(i => i.Kind == EnrichmentService.EpisodeKind.Episode))
    {
      if (episodes.TryGetValue(item.Name, out var episode))
      {
        await EnrichEpisodeAsync(
          episode, 
          await ReloadAsync(item.Id));
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
          AnsiConsole.Write($"[Red]FAILED: Unable to match {item.Name}[/]");
          AnsiConsole.WriteLine();
          continue;
        }
        AnsiConsole.Write($"[Yellow]Warning: No precise match {item.Name}[/]");
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<string>()
          .Title("Pick the [Blue]best match[/] or [Yellow]skip[/]:")
          .AddChoiceGroup("Matches", matches.Select(i => i.Title))
          .AddChoices(SKIP_CHOICE);

        var choice = AnsiConsole.Prompt(prompt);
        switch (choice)
        {
          case SKIP_CHOICE:
            AnsiConsole.WriteLine($"Skipping remote: {item.Name}");
            break;
          default:
            await EnrichEpisodeAsync(
              matches.First(i => i.Title == choice), 
              await ReloadAsync(item.Id));

            break;
        }
      }
    }
    
    async Task EnrichEpisodeAsync(
      EpisodeItem episode,
      EnrichmentService.Episode remoteEpisode)
    {
      remoteEpisode = (await this.enrichment.GetEpisodeAsync(remoteEpisode.Id))!;
      await FileServices.SaveAsync(
        new EpisodePropsItem
        {
          Date = remoteEpisode.Date,
          Summary = remoteEpisode.Overview,
          Directors = [.. (remoteEpisode.Characters ?? []).Where(i => i.PersonType == "Director").Select(i => i.PersonName)],
          Writers = [.. (remoteEpisode.Characters ?? []).Where(i => i.PersonType == "Writer").Select(i => i.PersonName)],
        }, 
        new FilePropsFilePath(episode.Path.Value));
    }

    async Task<EnrichmentService.Episode> ReloadAsync(
      long remoteId)
    {
      return (await this.enrichment.GetEpisodeAsync(remoteId))!;
    }
  }
}
