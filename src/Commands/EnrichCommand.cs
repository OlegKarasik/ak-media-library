using System.Diagnostics.CodeAnalysis;
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
      AnsiConsole.MarkupLineInterpolated($"[[[Red]{showItem.Title}[/]]]: No matches found");
      return;
    }

    var selection = this.PickMatchingRemoteSeries(showItem, search);
    if (selection is null)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Yellow]{showItem.Title}[/]]]: Skipped");
      return;
    }

    var series = await AnsiConsole
      .Status()
      .StartAsync($"[Bold][[{showItem.Title}]][/]: Downloading...", async ctx => await this.enrichment.GetSeriesAsync(selection.Id, settings.Language));

    if (series is not null)
    {
      await this.SaveInformation(showItem, series);

      this.statistics.WriteShowEnriched();

      foreach (var seasonItem in showItem.Seasons)
      {
        var season = await this.PickMatchingRemoteSeason(seasonItem, series, settings);
        if (season is not null)
        {
          await this.EnrichSeasonItemAsync(seasonItem, season);

          foreach (var episodeItem in seasonItem.Episodes)
          {
            var episode = await this.PickMatchingRemoteEpisode(episodeItem, season, settings);
            if (episode is not null)
            {
              await this.EnrichEpisodeItemAsync(episodeItem, episode);
            }
          }
        }
      }
    }
  }

  private Search? PickMatchingRemoteSeries(
      ShowItem show,
      Search[] search)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(search);

    for (; ;)
    {
      var promptSelection = new SelectionPrompt<Search>()
        .Title(
          $"""
          [Bold][[{show.Title}]][/]: Found [Green]{search.Length}[/] potential matches

          [Cyan]Select a show to preview[/]
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

          [Cyan]Continue?[/]
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

  private Task<Season?> PickMatchingRemoteSeason(
    SeasonItem seasonItem,
    Series series,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(seasonItem);
    ArgumentNullException.ThrowIfNull(series);
    ArgumentNullException.ThrowIfNull(settings);

    var position = (long)seasonItem.Position.GetPosition();
    if (series.Seasons.TryGetValue((long)seasonItem.Position.GetPosition(), out var remoteSeason))
    {
      return Task.FromResult<Season?>(remoteSeason);
    }

    AnsiConsole.MarkupLineInterpolated($"[Red]E[/]: Unable to match [Bold]Season {position}[/]");
    return Task.FromResult<Season?>(null);
  }

  private Task<Episode?> PickMatchingRemoteEpisode(
    EpisodeItem episodeItem,
    Season season,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(episodeItem);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(settings);

    if (season.Episodes.TryGetValue(episodeItem.Title, out var remoteEpisode))
    {
      return Task.FromResult<Episode?>(remoteEpisode);
    }

    var matches = season.Episodes.Values
      .Where(i => episodeItem.Title.FuzzyMatch(i.Title, settings.MaxFuzzyCharacters))
      .ToArray();

    if (matches.Length == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[Red]E[/]: Unable to match [Bold]{episodeItem.Title}[/]");
      return Task.FromResult<Episode?>(null);
    }

    for (; ; )
    {
      var (result, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(int, string display)>()
          .Title($"[Bold]{episodeItem.Title}[/] >>> (?)")
          .UseConverter(item => item.display)
          .AddChoices(
            [..
              matches.Select((item, index) => (index, $"{item.Title} (Season {item.SeasonIndex}, {item.Date})")),
              (-1, "[Yellow]Skip[/]")
            ]
          ));

      // Skip
      //
      if (result == -1)
      {
        AnsiConsole.MarkupLineInterpolated($"[Yellow]S[/]: Skip [Bold]{episodeItem.Title}[/]");
        return Task.FromResult<Episode?>(null);
      }

      return Task.FromResult<Episode?>(matches[result]);
    }
  }

  private async Task SaveInformation(
    ShowItem show,
    Series remoteShow)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(remoteShow);

    if (remoteShow.Image is not null)
    {
      await SaveAsync(
        remoteShow.Image, new FilePathImage(show.Path));
    }
    if (remoteShow.ImageBackground is not null)
    {
      await SaveAsync(
        remoteShow.ImageBackground, new FilePathImageBackground(show.Path));
    }

    await SaveAsync(
      new ShowPropsItem
      {
        Summary = [remoteShow.Overview],
        Date = remoteShow.Date,
        Genres = remoteShow.Genres
      },
      new FilePathProps(show.Path));
  }

  private async Task EnrichSeasonItemAsync(
    SeasonItem season,
    Season remoteSeason)
  {
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(remoteSeason);

    if (remoteSeason.Image is not null)
    {
      await SaveAsync(
        remoteSeason.Image, new FilePathImage(season.Path));
    }
    if (remoteSeason.ImageBackground is not null)
    {
      await SaveAsync(
        remoteSeason.ImageBackground, new FilePathImageBackground(season.Path));
    }

    await SaveAsync(
      new SeasonPropsItem
      {
        Summary = [remoteSeason.Overview],
      },
      new FilePathProps(season.Path));

    this.statistics.WriteSeasonEnriched();
  }

  private async Task EnrichEpisodeItemAsync(
    EpisodeItem episode,
    Episode remoteEpisode)
  {
    ArgumentNullException.ThrowIfNull(episode);
    ArgumentNullException.ThrowIfNull(remoteEpisode);

    await SaveAsync(
      new EpisodePropsItem
      {
        Date = remoteEpisode.Date,
        Summary = [remoteEpisode.Overview],
        Directors = [.. remoteEpisode.Directors.Select(i => i.Name)],
        Writers = [.. remoteEpisode.Writers.Select(i => i.Name)],
      },
      new FilePathProps(episode.Path));

    this.statistics.WriteEpisodeEnriched();
  }
}
