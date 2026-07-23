using MediaLibrary.Business;
using MediaLibrary.Business.Enrichment;
using MediaLibrary.Business.Enrichment.Models;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Commands.Base;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class EnrichCommand : MediaCommand<EnrichCommandSettings>
{
  private class EnrichmentPath
  {
    private readonly EnrichmentPath? parent;

    public readonly string Value;

    public EnrichmentPath(
      string value)

      : this(null, value)
    {
    }

    public EnrichmentPath(
      EnrichmentPath? parent, 
      string value)
    {
      ArgumentNullException.ThrowIfNull(value);
      
      this.Value = value;

      this.parent = parent;
    }

    public override string ToString()
    {
      return $"{parent} / {this.Value}";
    }
  }

  private class EnrichCommandStatistics
  {
    public bool HasEnriched => this.Enriched != 0;

    public long Enriched => this.ShowsEnriched + this.SeasonsEnriched + this.EpisodesEnriched;
    public long ShowsEnriched { get; private set; }
    public long SeasonsEnriched { get; private set; }
    public long EpisodesEnriched { get; private set; }

    public void WriteShowEnriched()
    {
      this.ShowsEnriched++;
    }
    public void WriteSeasonEnriched()
    {
      this.SeasonsEnriched++;
    }
    public void WriteEpisodeEnriched()
    {
      this.EpisodesEnriched++;
    }
  }

  private readonly EnrichCommandOptions options;
  private readonly EnrichCommandStatistics statistics;
  private readonly EnrichmentService enrichment;

  public EnrichCommand(
    EnrichmentService enrichment)
  {
    this.options = new EnrichCommandOptions();
    this.statistics = new EnrichCommandStatistics();
    this.enrichment = enrichment ?? throw new ArgumentNullException(nameof(enrichment));
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context,
    EnrichCommandSettings settings,
    CancellationToken cancellationToken)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () =>
        {
          var index = await GetAsync(new FilePathIndex(settings.Library));
          switch (IndexSearch.GetItem(index, settings.SearchRequest))
          {
            case ShowCollectionItem collection:
              {
                foreach (var show in collection.Shows)
                {
                  await this.ProcessShow(show, settings);
                }
                break;
              }
            case ShowItem show:
              {
                await this.ProcessShow(show, settings);
              }
              break;
            default:
              throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
          }
        });

    if (this.statistics.HasEnriched)
    {
      AnsiConsole.Write(
        new BarChart()
        .AddItems(
          [
            new BarChartItem("Shows", this.statistics.ShowsEnriched, Color.DarkGoldenrod),
            new BarChartItem("Seasons", this.statistics.SeasonsEnriched, Color.Aqua),
            new BarChartItem("Episodes", this.statistics.EpisodesEnriched, Color.DarkMagenta)
          ]));

      AnsiConsole.Write(new Rule());
      AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Enriched - [Underline]{this.statistics.Enriched}[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
    }
    else
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Enriched - [Underline]None[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
    }


    return 0;
  }

  private async Task ProcessShow(
    ShowItem showItem,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(showItem);
    ArgumentNullException.ThrowIfNull(settings);

    var showPropsItem = await GetAsync<ShowPropsItem>(new FilePathProps(showItem.Path)) ?? new ShowPropsItem();

    await this.UpdateShowProps(showPropsItem, showItem);

    var series = await this.PickMatchingRemoteSeries(showPropsItem, showItem, settings);
    if (series.val is not null)
    {
      await this.UpdateShowProps(showPropsItem, series.val);

      if (series.val.Image is not null)
      {
        await SaveAsync(series.val.Image, new FilePathImage(showItem.Path));
      }
      if (series.val.ImageBackground is not null)
      {
        await SaveAsync(series.val.ImageBackground, new FilePathImageBackground(showItem.Path));
      }

      this.statistics.WriteShowEnriched();
    }
    else
    {
      if (series.skip)
      {
        AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{showItem.Title}\"[/]");
      }
      else
      {
        AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{showItem.Title}\"[/]");
      }
    }

    await SaveAsync(showPropsItem, new FilePathProps(showItem.Path));

    if (series.val is null)
    {
      return;
    }

    foreach (var seasonItem in showItem.Seasons)
    {
      var seasonPropsItem = await GetAsync<SeasonPropsItem>(new FilePathProps(seasonItem.Path)) ?? new SeasonPropsItem();

      await this.UpdateSeasonProps(seasonPropsItem, seasonItem);

      var season = this.PickMatchingRemoteSeason(seasonPropsItem, seasonItem, series.val, settings);

      if (season.val is not null)
      {
        await this.UpdateSeasonProps(seasonPropsItem, season.val);

        if (season.val.Image is not null)
        {
          await SaveAsync(season.val.Image, new FilePathImage(seasonItem.Path));
        }
        if (season.val.ImageBackground is not null)
        {
          await SaveAsync(season.val.ImageBackground, new FilePathImageBackground(seasonItem.Path));
        }

        this.statistics.WriteSeasonEnriched();
      }
      else
      {
        if (season.skip)
        {
          AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{seasonItem.Title}\"[/]");
        }
        else
        {
          AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{seasonItem.Title}\"[/]");
        }
      }

      await SaveAsync(seasonPropsItem, new FilePathProps(seasonItem.Path));

      if (season.val is null)
      {
        continue;
      }

      foreach (var episodeItem in seasonItem.Episodes)
      {
        var episodePropsItem = await GetAsync<EpisodePropsItem>(new FilePathProps(episodeItem.Path)) ?? new EpisodePropsItem();

        await this.UpdateEpisodeProps(episodePropsItem, episodeItem);
        
        var episode = this.PickMatchingRemoteEpisode(episodePropsItem, episodeItem, season.val, settings);

        if (episode.val is not null)
        {
          await this.UpdateEpisodeProps(episodePropsItem, episode.val);

          this.statistics.WriteEpisodeEnriched();
        }
        else
        {
          if (episode.skip)
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{episodeItem.Title}\"[/]");
          }
          else
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{episodeItem.Title}\"[/]");
          }
        }

        await SaveAsync(episodePropsItem, new FilePathProps(episodeItem.Path));

        if (episode.val is null)
        {
          continue;
        }
      }
    }
  }

  private async Task<(Series? val, bool skip)> PickMatchingRemoteSeries(
    ShowPropsItem showPropsItem,
    ShowItem showItem,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(showPropsItem);
    ArgumentNullException.ThrowIfNull(showItem);
    ArgumentNullException.ThrowIfNull(settings);

    var search =
      await AnsiConsole
        .Status()
        .StartAsync($"[Bold][[{showItem.Title}]][/]: Searching matches...", async ctx => await this.enrichment.SearchSeriesAsync(showItem.Title, settings.Language));

    if (search.Length == 0)
    {
      return (null, false);
    }
    
    Search? selection = null;
    if (showPropsItem.MemoryTitle is not null && showPropsItem.MemoryYear is not null)
    {
      selection = search.FirstOrDefault(i => i.Title == showPropsItem.MemoryTitle && i.Year == showPropsItem.MemoryYear);
    }

    for (; selection is null;)
    {
      var promptSelection = new SelectionPrompt<Search>()
        .Title(
          $"""
          [Bold][[{showItem.Title}]][/]: Found [Green]{search.Length}[/] potential matches

          [Cyan]Select to see details[/]
          [Gray](Hit [Underline]ESCAPE[/] to cancel the prompt and [Underline]SKIP[/] the show)[/]
          """)
        .UseConverter(search => 
          $"""
          [Bold]{search.Title} ({search.Year})[/]
          """)
        .PageSize(5)
        .AddChoices(search);

      if (!AnsiConsole.TryPrompt(promptSelection, out selection))
      {
        return (null, true);
      }

      var promptMatch = new SelectionPrompt<bool>()
        .Title(
          $"""
          [Bold]{selection.Title} ({selection.Year})[/]
          {(string.IsNullOrEmpty(selection.Overview) ? "N/A (no overview available)" : selection.Overview)}

          [Cyan]Confirm?[/]
          """)
        .UseConverter(value => value switch { true => "Yes", false => "No" })
        .AddChoices([true, false]);

      if (!AnsiConsole.TryPrompt(promptMatch, out var confirmation) || !confirmation)
      {
        selection = null;
        continue;
      }
    }

    var series = await AnsiConsole
      .Status()
      .StartAsync($"[Bold][[{selection.Title}]][/]: Downloading...", async ctx => await this.enrichment.GetSeriesAsync(selection.Id, settings.Language));

    if (series is null)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unable to download [Bold]\"{showItem.Title}\"[/]. Please retry");
    }
    return (series, false);
  }

  private (Season? val, bool skip) PickMatchingRemoteSeason(
    SeasonPropsItem seasonPropsItem,
    SeasonItem seasonItem,
    Series series,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(seasonPropsItem);
    ArgumentNullException.ThrowIfNull(seasonItem);
    ArgumentNullException.ThrowIfNull(series);
    ArgumentNullException.ThrowIfNull(settings);

    var position = (long)seasonItem.Position.GetPosition();
    if (series.Seasons.TryGetValue(position, out var season))
    {
      return (season, false);
    }
    if (seasonPropsItem.MemoryPosition is not null)
    {
      if (series.Seasons.TryGetValue((long)seasonPropsItem.MemoryPosition.GetPosition(), out season))
      {
        return (season, false);
      }
    }

    return (null, false);
  }

  private (Episode? val, bool skip) PickMatchingRemoteEpisode(
    EpisodePropsItem episodePropsItem,
    EpisodeItem episodeItem,
    Season season,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(episodePropsItem);
    ArgumentNullException.ThrowIfNull(episodeItem);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(settings);

    if (season.Episodes.TryGetValue(episodeItem.Title, out var episode))
    {
      return (episode, false);
    }
    if (episodePropsItem.MemoryTitle is not null)
    {
      if (season.Episodes.TryGetValue(episodePropsItem.MemoryTitle, out episode))
      {
        return (episode, false);
      }
    }

    if (settings.DisableFuzzy)
    {
      return (null, false);
    }

    var matches = season.Episodes.Values
      .Where(i => episodeItem.Title.FuzzyMatch(i.Title, settings.MaxFuzzyCharacters))
      .ToArray();

    if (matches.Length == 0)
    {
      return (null, false);
    }

    for (; ; )
    {
      var promptSelection = new SelectionPrompt<Episode>()
          .Title(
            $"""
             [Bold][[{episodeItem.Title}]][/]: Found [Green]{matches.Length}[/] potential matches

             [Cyan]Select to see details[/]
             [Gray](Hit [Underline]ESCAPE[/] to cancel the prompt and [Underline]SKIP[/] the episode)[/]
             """)
          .UseConverter(episode =>
            $"""
             [Bold]{episode.Title}[/]
             """)
          .PageSize(5)
          .AddChoices(matches);

      if (!AnsiConsole.TryPrompt(promptSelection, out var selection))
      {
        return (null, true);
      }

      var promptMatch = new SelectionPrompt<bool>()
        .Title(
          $"""
          [Bold]{selection.Title}[/]
          {(string.IsNullOrEmpty(selection.Overview) ? "N/A (no overview available)" : selection.Overview)}

          [Cyan]Confirm?[/]
          """)
        .UseConverter(value => value switch { true => "Yes", false => "No" })
        .AddChoices([true, false]);

      if (!AnsiConsole.TryPrompt(promptMatch, out var confirmation) || !confirmation)
      {
        continue;
      }

      return (selection, false);
    }
  }

  private async Task UpdateShowProps(
    ShowPropsItem showPropsItem,
    ShowItem showItem)
  {
    ArgumentNullException.ThrowIfNull(showPropsItem);
    ArgumentNullException.ThrowIfNull(showItem);

    showPropsItem.Title = showItem.Title.ToString();
  }

  private async Task UpdateShowProps(
    ShowPropsItem showPropsItem,
    Series series)
  {
    ArgumentNullException.ThrowIfNull(showPropsItem);
    ArgumentNullException.ThrowIfNull(series);

    showPropsItem.Summary = [series.Overview];
    showPropsItem.Date = series.Date;
    showPropsItem.Genres = series.Genres;
    showPropsItem.MemoryTitle = series.Title;
    showPropsItem.MemoryYear = series.Year;
  }

  private async Task UpdateSeasonProps(
    SeasonPropsItem seasonPropsItem,
    SeasonItem seasonItem)
  {
    ArgumentNullException.ThrowIfNull(seasonPropsItem);
    ArgumentNullException.ThrowIfNull(seasonItem);

    seasonPropsItem.Title = seasonItem.Title.ToString();
  }

  private async Task UpdateSeasonProps(
    SeasonPropsItem seasonPropsItem,
    Season season)
  {
    ArgumentNullException.ThrowIfNull(seasonPropsItem);
    ArgumentNullException.ThrowIfNull(season);

    seasonPropsItem.Summary = [season.Overview];
    seasonPropsItem.MemoryPosition = new ItemPosition((ulong)season.Index);
  }

  private async Task UpdateEpisodeProps(
    EpisodePropsItem episodePropsItem,
    EpisodeItem episodeItem)
  {
    ArgumentNullException.ThrowIfNull(episodePropsItem);
    ArgumentNullException.ThrowIfNull(episodeItem);

    episodePropsItem.Title = episodeItem.Title.ToString();
  }

  private async Task UpdateEpisodeProps(
    EpisodePropsItem episodePropsItem,
    Episode episode)
  {
    ArgumentNullException.ThrowIfNull(episodePropsItem);
    ArgumentNullException.ThrowIfNull(episode);

    episodePropsItem.Date        = episode.Date;
    episodePropsItem.Summary     = [episode.Overview];
    episodePropsItem.Directors   = [.. episode.Directors.Select(i => i.Name)];
    episodePropsItem.Writers     = [.. episode.Writers.Select(i => i.Name)];
    episodePropsItem.MemoryTitle = episode.Title;
  }

  private async Task SaveEpisodeArtefacts(
    EpisodeItem episodeItem,
    Episode episode)
  {
    ArgumentNullException.ThrowIfNull(episodeItem);
    ArgumentNullException.ThrowIfNull(episode);
  }
}
