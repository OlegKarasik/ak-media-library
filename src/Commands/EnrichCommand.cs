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
  private const int FUZZY_MATCH_CONSTANT = 10;

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
          var _show = await this.PickSeriesMatchAsync(show.Title);
          if (_show is null)
          {
            return -1;
          }

          AnsiConsoleService.Rule(_show.Name);

          // TODO: Enrich series information
          //

          foreach (var season in show.Seasons.Values)
          {
            var _season = await this.PickSeasonMatchAsync((long)season.Position.GetPosition(), _show);
            if (_season is null)
            {
              return -1;
            }

            AnsiConsoleService.Rule($"Season {_season.Index}");

            // TODO: Enrich season information
            //

            foreach (var episode in season.Episodes.Values)
            {
              var _episode = await this.PickEpisodeMatchAsync(episode.Title, _season);
              if (_episode is null)
              {
                continue;
              }

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

  private async Task<long?> PickMatchAsync(
    string lookupTitle,
    EnrichmentService.SearchTarget lookupTarget)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync("Searching matches", async ctx => await this.enrichment.SearchAsync(lookupTitle, lookupTarget)));

    if (measurement.Data is null)
    {
      AnsiConsole.MarkupLineInterpolated($"No entries matching [Green]{lookupTitle}[/] found. Elapsed [Green]{measurement.Elapsed}[/]");
      return null;
    }

    AnsiConsole.MarkupLineInterpolated($"Found [Green]{measurement.Data.Length}[/] matching entries. Elapsed [Green]{measurement.Elapsed}[/]");
    
    for (;;)
    {
      var match = AnsiConsoleService.Print(
        AnsiConsoleService.Select(measurement.Data, i => $"{i.Name} ({i.Year})"));

      switch (AnsiConsoleService.SelectContinueBack())
      {
        case AnsiConsoleService.ContinueBack.Back:
          continue;
        default:
          break;
      }
      return match.Id;
    }
  }

  private async Task<EnrichmentService.Series?> PickSeriesMatchAsync(
    string lookupTitle)
  {
    var lookupId = await this.PickMatchAsync(lookupTitle, EnrichmentService.SearchTarget.Series);
    if (lookupId is null)
    {
      return null;
    }

    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync($"Loading [Green]{lookupTitle}[/]", async ctx => await this.enrichment.GetSeriesAsync(lookupId.Value)));

    if (measurement.Data is not null)
    {
      AnsiConsole.MarkupLineInterpolated($"Loaded information about [Green]{lookupTitle}[/] series. Elapsed [Green]{measurement.Elapsed}[/]");
    }
    return measurement.Data;
  }

  private async Task<EnrichmentService.Season?> PickSeasonMatchAsync(
    long lookupIndex,
    EnrichmentService.Series series)
  {
    var season = series.Seasons
      .Where(i => i.Index == lookupIndex)
      .FirstOrDefault();
    
    if (season is null)
    {
      return null;
    }
    
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync($"Loading [Green]Season {lookupIndex}[/]", async ctx => await this.enrichment.GetSeasonAsync(season.Id)));
    
    if (measurement.Data is not null)
    {
      AnsiConsole.MarkupLineInterpolated($"Loaded information about [Green]Season {lookupIndex}[/]. Elapsed [Green]{measurement.Elapsed}[/]");
    }
    return measurement.Data;
  }

  private async Task<EnrichmentService.Episode?> PickEpisodeMatchAsync(
    string lookupTitle,
    EnrichmentService.Season season)
  {
    EnrichmentService.Season.Episode? match = null;
    foreach (var episode in season.Episodes.Where(i => i.Kind == EnrichmentService.EpisodeKind.Episode))
    {
      if (lookupTitle.Equals(episode.Name, StringComparison.OrdinalIgnoreCase))
      {
        match = episode;
        break;
      }
    }

    if (match is null)
    {
      var fuzzy = new List<EnrichmentService.Season.Episode>();
      foreach (var episode in season.Episodes.Where(i => i.Kind == EnrichmentService.EpisodeKind.Episode))
      {
        if (lookupTitle.CalculateLevenshteinDistance(episode.Name) < FUZZY_MATCH_CONSTANT)
        {
          fuzzy.Add(episode);
        }
      }
      if (fuzzy.Count == 0)
      {
        AnsiConsole.MarkupLineInterpolated($"[Red]FAILED[/] to match remote episodes from [Green]Season {season.Index}[/] to [Green]{lookupTitle}[/]");
        return null;
      }

      AnsiConsole.MarkupLineInterpolated($"Found [Green]{fuzzy.Count}[/] episodes which more or less match [Red]{lookupTitle}[/]");

      for (;;)
      {
        match = AnsiConsoleService.Print(
          AnsiConsoleService.Select(fuzzy, i => $"{i.Name} (Season {season.Index}, {season.Year})"));

        switch (AnsiConsoleService.SelectContinueBackSkip())
        {
          case AnsiConsoleService.ContinueBackSkip.Skip:
            match = null;

            AnsiConsole.MarkupLineInterpolated($"[YELLOW]SKIPPED[/] matching of [Green]{lookupTitle}[/] episode");
            break;
          case AnsiConsoleService.ContinueBackSkip.Back:
            continue;
          default:
            break;
        }
        break;
      }
    }
    if (match is not null)
    {
      var measurement = await TimeServices.MeasureAsync(
        async () => 
          await AnsiConsole
            .Status()
            .StartAsync($"Loading {lookupTitle}", async ctx => await this.enrichment.GetEpisodeAsync(match.Id)));
      
      if (measurement.Data is not null)
      {
        AnsiConsole.MarkupLineInterpolated($"Loaded information about [Green]{lookupTitle}[/] episode. Elapsed [Green]{measurement.Elapsed}[/]");
      }

      return measurement.Data;
    }
    return null;
  }
    
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
