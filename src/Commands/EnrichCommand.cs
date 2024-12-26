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
          var _show = await this.PickSeriesMatchAsync(show.Title);
          if (_show is null)
          {
            return -1;
          }

          // TODO: Enrich series information
          //

          foreach (var season in show.Seasons.Values)
          {
            // TODO: Pick season
            //

            // TODO: Enrich season information
            //

            foreach (var episode in season.Episodes.Values)
            {
              var _episode = await this.PickEpisodeMatchAsync(episode.Title, _show);
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

  private async Task<long> PickMatchAsync(
    string lookupTitle,
    EnrichmentService.SearchTarget lookupTarget)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync("Searching matches", async ctx => await this.enrichment.SearchAsync(lookupTitle, lookupTarget)));

    AnsiConsole.MarkupLineInterpolated($"Found [Green]{measurement.Data.Length}[/] series. Elapsed [Green]{measurement.Elapsed}[/]");
    
    for (;;)
    {
      var match = AnsiConsole.Prompt(
        new SelectionPrompt<EnrichmentService.SearchResult>()
          .Title($"Select match to continue or use CTRL+C to cancel:")
          .UseConverter(i => $"{i.Name} ({i.Year ?? "[Red]N/A[/]"})")
          .AddChoices(measurement.Data));

      AnsiConsole.MarkupLineInterpolated($"Selected [Green]{match.Name}[/]");

      AnsiConsole.Write(
        new Panel(new Text(match.Overview ?? "[Red]N/A[/]"))
          .Header(match.Name.ToUpper(), Justify.Left));

      switch (
        AnsiConsole.Prompt(
          new SelectionPrompt<string>()
            .AddChoices("Continue", "Back")))
      {
        case "Continue":
          return match.Id;
        case "Back":
          break;
      }
    }
  }

  private async Task<EnrichmentService.Series?> PickSeriesMatchAsync(
    string lookupTitle)
  {
    var id = await this.PickMatchAsync(lookupTitle, EnrichmentService.SearchTarget.Series);

    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync($"Loading {lookupTitle}", async ctx => await this.enrichment.GetSeriesAsync(id)));

    if (measurement.Data is not null)
    {
      AnsiConsole.MarkupLineInterpolated($"Loaded information about [Green]{lookupTitle}[/] series. Elapsed [Green]{measurement.Elapsed}[/]");
    }
    return measurement.Data;
  }

  private async Task<EnrichmentService.Episode?> PickEpisodeMatchAsync(
    string lookupTitle,
    EnrichmentService.Series series)
  {
    EnrichmentService.Episode? match = null;
    foreach (var episode in (series.Episodes ?? []).Where(i => i.Kind == EnrichmentService.EpisodeKind.Episode))
    {
      if (lookupTitle.Equals(episode.Name, StringComparison.OrdinalIgnoreCase))
      {
        match = episode;
        break;
      }
    }

    if (match is null)
    {
      var fuzzy = new List<EnrichmentService.Episode>();
      foreach (var episode in (series.Episodes ?? []).Where(i => i.Kind == EnrichmentService.EpisodeKind.Episode))
      {
        if (lookupTitle.CalculateLevenshteinDistance(episode.Name) < FUZZY_MATCH_CONSTANT)
        {
          fuzzy.Add(episode);
        }
      }
      if (fuzzy.Count == 0)
      {
        AnsiConsole.MarkupLineInterpolated($"[Red]FAILED[/]: No remote episode matches [Green]{lookupTitle}[/]");
        return null;
      }

      for (;;)
      {
        match = AnsiConsole.Prompt(
          new SelectionPrompt<EnrichmentService.Episode>()
            .Title($"No episodes matched [Green]{lookupTitle}[/] automatically, select match to continue:")
            .UseConverter(i => $"{i.Name} ({i.Year ?? "[Red]N/A[/]"})")
            .AddChoices(fuzzy));

        AnsiConsole.MarkupLineInterpolated($"Selected [Green]{match.Name}[/]");

        AnsiConsole.Write(
          new Panel(
            new Rows(
              new Text(string.Empty),
              new Text(match.Overview ?? "[Red]N/A[/]"),
              new Text(string.Empty)))
            .Header(match.Name.ToUpper(), Justify.Left));

        switch (
          AnsiConsole.Prompt(
            new SelectionPrompt<string>()
              .AddChoices("Continue", "Skip", "Back")))
        {
          case "Continue":
            break;
          case "Back":
            continue;
          case "Skip":
            match = null;

            AnsiConsole.MarkupLineInterpolated($"Skipped matching of [Green]{lookupTitle}[/] episode");
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

      match = measurement.Data;
    }
    return match;
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
