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
  private int FUZZY_MATCH_CONSTANT = 6;

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
    var index = await AnsiConsole
        .Status()
        .StartAsync("Loading index", async ctx => await FileServices.LoadAsync(new DirectoryIndexFilePath(settings.Library.Value)));

    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case ShowItem show:
        {
          var remoteSeries = await this.PickSeriesMatchAsync(show.Title);
          if (remoteSeries is null)
          {
            return -1;
          }

          // TODO: Enrich series information
          //

          foreach (var season in show.Seasons.Values)
          {
            foreach (var episode in season.Episodes.Values)
            {
              // var remoteEpisode = await this.PickEpisodeMatchAsync(episode.Title, remoteEpisodes);
              // if (remoteEpisode is null)
              // {
              //   continue;
              // }

              // TODO: Enrich episode information
              //
            }
          }
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }

  

  private async Task<long> PickMatchAsync(
    string title,
    EnrichmentService.SearchTarget target)
  {
    var results = await AnsiConsole
      .Status()
      .StartAsync("Searching", async ctx => await this.enrichment.SearchAsync(title, target));
    
    for (;;)
    {
      var match = AnsiConsole.Prompt(
        new SelectionPrompt<EnrichmentService.SearchResult>()
          .Title($"Found {results.Length} matches, select one to continue or use CTRL+C to cancel:")
          .UseConverter(i => $"{i.Name} ({i.Year ?? "N/A"})")
          .AddChoices(results));

      AnsiConsole.Write(
        new Panel(
          new Rows(
            new Markup("Overview"),
            new Text(match.Overview ?? "N/A")))
          .Header(match.Name.ToUpper(), Justify.Left));

      if (AnsiConsole.Prompt(new ConfirmationPrompt("Continue?")))
      {
        return match.Id;
      }
    }
  }

  private async Task<EnrichmentService.Series> PickSeriesMatchAsync(
    string seriesTitle)
  {
    var id = await this.PickMatchAsync(seriesTitle, EnrichmentService.SearchTarget.Series);
    return await AnsiConsole
      .Status()
      .StartAsync("Loading", async ctx => (await this.enrichment.GetSeriesAsync(id))!);
  }

  // private Task<EnrichmentService.Episode> PickEpisodeMatchAsync(
  //   string episodeTitle,
  //   IDictionary<string, EnrichmentService.Episode> remoteEpisodes)
  // {
  //   if (remoteEpisodes.TryGetValue(episodeTitle, out var episode))
  //   {
  //     return episode;
  //   }
  //   else
  //   {
  //     var fuzzy = new List<EpisodeItem>();
  //     foreach (var (title, value) in localEpisodesDictionary)
  //     {
  //       if (title.CalculateLevenshteinDistance(remoteEpisode.Name) < FUZZY_MATCH_CONSTANT)
  //       {
  //         fuzzy.Add(value);
  //       }
  //     }
  //     if (fuzzy.Count == 0)
  //     {
  //       AnsiConsole.MarkupLineInterpolated($"[Red]ERRO[/]: No local episode matches [Green]{remoteEpisode.Name}[/]");
  //       continue;
  //     }
  //     AnsiConsole.MarkupLineInterpolated($"[Yellow]WARN[/]: Multiple local episodes matches [Green]{remoteEpisode.Name}[/]");
  //     AnsiConsole.Write(
  //       new Rows(fuzzy.Select((i, index) => new Markup($"{index}. {i.Title}"))));

  //     if (AnsiConsole.Confirm("Pick one [Blue](y)[/] or skip [Blue](n)[/]?"))
  //     {
  //       var match = AnsiConsole.Prompt(
  //         new SelectionPrompt<EpisodeItem>()
  //           .Title("[Yellow]WARN[/]: Multiple local episodes matches [Green]{item.Name}[/], pick one or skip:")
  //           .AddChoices(fuzzy));

  //       yield return (match, remoteEpisode.Id);
  //     }
  //   }
  // }
    
  //   async Task EnrichEpisodeAsync(
  //     EpisodeItem episode,
  //     EnrichmentService.Episode remoteEpisode)
  //   {
  //     remoteEpisode = (await this.enrichment.GetEpisodeAsync(remoteEpisode.Id))!;
  //     await FileServices.SaveAsync(
  //       new EpisodePropsItem
  //       {
  //         Date = remoteEpisode.Date,
  //         Summary = remoteEpisode.Overview,
  //         Directors = [.. (remoteEpisode.Characters ?? []).Where(i => i.PersonType == "Director").Select(i => i.PersonName)],
  //         Writers = [.. (remoteEpisode.Characters ?? []).Where(i => i.PersonType == "Writer").Select(i => i.PersonName)],
  //       }, 
  //       new FilePropsFilePath(episode.Path.Value));
  //   }

  //   async Task<EnrichmentService.Episode> ReloadAsync(
  //     long remoteId)
  //   {
  //     return (await this.enrichment.GetEpisodeAsync(remoteId))!;
  //   }
}
