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

    var search =
      await AnsiConsole
        .Status()
        .StartAsync($"[Bold][[{showItem.Title}]][/]: Searching matches...", async ctx => await this.enrichment.SearchSeriesAsync(showItem.Title, settings.Language));

    if (search.Length == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{showItem.Title}\"[/]");
      return;
    }

    var selection = this.PickMatchingRemoteSeries(showItem, search);
    if (selection is null)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{showItem.Title}\"[/]");
      return;
    }

    var series = await AnsiConsole
      .Status()
      .StartAsync($"[Bold][[{selection.Title}]][/]: Downloading...", async ctx => await this.enrichment.GetSeriesAsync(selection.Id, settings.Language));

    if (series is null)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unable to download [Bold]\"{showItem.Title}\"[/]. Please retry");
      return;
    }

    await this.SaveInformation(showItem, series);

    this.statistics.WriteShowEnriched();

    foreach (var seasonItem in showItem.Seasons)
    {
      var season = this.PickMatchingRemoteSeason(seasonItem, series, settings);

      await this.SaveInformation(seasonItem, season.val);

      if (season.val is null)
      {
        if (season.skip)
        {
          AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{seasonItem.Title}\"[/]");
        }
        else
        {
          AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{seasonItem.Title}\"[/]");
        }
        continue;
      }

      this.statistics.WriteSeasonEnriched();

      foreach (var episodeItem in seasonItem.Episodes)
      {
        var episode = this.PickMatchingRemoteEpisode(episodeItem, season.val, settings);

        await this.SaveInformation(episodeItem, episode.val);

        if (episode.val is null)
        {
          if (episode.skip)
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipped [Bold]\"{episodeItem.Title}\"[/]");
          }
          else
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Red]E[/]]]: Unmatched [Bold]\"{episodeItem.Title}\"[/]");
          }
          continue;
        }

        this.statistics.WriteEpisodeEnriched();
      }
    }
  }

  private Search? PickMatchingRemoteSeries(
      ShowItem showItem,
      Search[] search)
  {
    ArgumentNullException.ThrowIfNull(showItem);
    ArgumentNullException.ThrowIfNull(search);

    for (; ;)
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

      if (!AnsiConsole.TryPrompt(promptSelection, out var selection))
      {
        return null;
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
        continue;
      }

      return selection;
    }
  }

  private (Season? val, bool skip) PickMatchingRemoteSeason(
    SeasonItem seasonItem,
    Series series,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(seasonItem);
    ArgumentNullException.ThrowIfNull(series);
    ArgumentNullException.ThrowIfNull(settings);

    var position = (long)seasonItem.Position.GetPosition();
    if (series.Seasons.TryGetValue(position, out var season))
    {
      return (season, false);
    }

    return (null, false);
  }

  private (Episode? val, bool skip) PickMatchingRemoteEpisode(
    EpisodeItem episodeItem,
    Season season,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(episodeItem);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(settings);

    if (season.Episodes.TryGetValue(episodeItem.Title, out var episode))
    {
      return (episode, false);
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

  private async Task SaveInformation(
    ShowItem showItem,
    Series? series)
  {
    ArgumentNullException.ThrowIfNull(showItem);

    var props = new ShowPropsItem
      {
        Title = showItem.Title.ToString()
      };

    if (series is not null)
    {
      props = new ShowPropsItem
        {
          Title = props.Title,
          Summary = [series.Overview],
          Date = series.Date,
          Genres = series.Genres
        };
      
      if (series.Image is not null)
      {
        await SaveAsync(
          series.Image, new FilePathImage(showItem.Path));
      }
      if (series.ImageBackground is not null)
      {
        await SaveAsync(
          series.ImageBackground, new FilePathImageBackground(showItem.Path));
      }
    }

    await SaveAsync(
      props,
      new FilePathProps(showItem.Path));
  }

  private async Task SaveInformation(
    SeasonItem seasonItem,
    Season? season)
  {
    ArgumentNullException.ThrowIfNull(seasonItem);

    var props = new SeasonPropsItem
      {
        Title = seasonItem.Title.ToString(),
      };

    if (season is not null)
    {
      props = new SeasonPropsItem
        {
          Title = props.Title,
          Summary = [season.Overview],
        };

      if (season.Image is not null)
      {
        await SaveAsync(
          season.Image, new FilePathImage(seasonItem.Path));
      }
      if (season.ImageBackground is not null)
      {
        await SaveAsync(
          season.ImageBackground, new FilePathImageBackground(seasonItem.Path));
      }
    }

    await SaveAsync(
      props,
      new FilePathProps(seasonItem.Path));

    this.statistics.WriteSeasonEnriched();
  }

  private async Task SaveInformation(
    EpisodeItem episodeItem,
    Episode? episode)
  {
    ArgumentNullException.ThrowIfNull(episodeItem);

    var props = new EpisodePropsItem
      {
        Title = episodeItem.Title.ToString(),
      };
    
    if (episode is not null)
    {
      props = new EpisodePropsItem
        {
          Title = props.Title,
          Date = episode.Date,
          Summary = [episode.Overview],
          Directors = [.. episode.Directors.Select(i => i.Name)],
          Writers = [.. episode.Writers.Select(i => i.Name)],
        };
    }

    await SaveAsync(
      props,
      new FilePathProps(episodeItem.Path));

    this.statistics.WriteEpisodeEnriched();
  }
}
