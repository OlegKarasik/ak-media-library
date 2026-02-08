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
    ShowItem show,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(settings);

    var search =
      await AnsiConsole
        .Status()
        .StartAsync("Searching...", async ctx => await this.enrichment.SearchSeriesAsync(show.Title, settings.Language));

    if (search.Length == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Red]F[/]]]: No results found matching the [Bold]{show.Title}[/] show");
      return;
    }

    var prompt = new SelectionPrompt<Search>()
      .Title(
        $"""
        Found [Green]{search.Length}[/] potential sources to enrich [Green Bold][[{show.Title}]][/], please select one to use
        [Gray](Hit [Underline]ESCAPE[/] to cancel the prompt and [Underline]SKIP[/] the show)[/]
        """)
      .UseConverter(search => 
        $"""
          [Bold]{search.Title} ({search.Year})[/]
            {(string.IsNullOrEmpty(search.Overview) ? "N/A" : search.Overview)}
          """)
      .PageSize(5)
      .AddChoices(search);

    if (!AnsiConsole.TryPrompt(prompt, out var selection))
    {
      AnsiConsole.MarkupLineInterpolated($"[[[Yellow]W[/]]]: Skipping enrichment of [Bold]{show.Title}[/] show");
      return;
    }

    var series = await AnsiConsole
      .Status()
      .StartAsync($"Downloading...", async ctx => await this.enrichment.GetSeriesAsync(selection.Id, settings.Language));

    if (series is not null)
    {
      await this.SaveInformation(show, series);

      this.statistics.WriteShowEnriched();

      foreach (var season in show.Seasons)
      {
        var remoteSeason = await this.PickMatchingRemoteSeason(series, season, settings);
        if (remoteSeason is not null)
        {
          await this.EnrichSeasonItemAsync(season, remoteSeason);

          foreach (var episode in season.Episodes)
          {
            var remoteEpisode = await this.PickMatchingRemoteEpisode(remoteSeason, episode, settings);
            if (remoteEpisode is not null)
            {
              await this.EnrichEpisodeItemAsync(episode, remoteEpisode);
            }
          }
        }
      }
    }
  }

  private Task<Season?> PickMatchingRemoteSeason(
    Series remoteSeries,
    SeasonItem season,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(remoteSeries);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(settings);

    var position = (long)season.Position.GetPosition();
    if (remoteSeries.Seasons.TryGetValue((long)season.Position.GetPosition(), out var remoteSeason))
    {
      return Task.FromResult<Season?>(remoteSeason);
    }

    AnsiConsole.MarkupLineInterpolated($"[Red]E[/]: Unable to match [Bold]Season {position}[/]");
    return Task.FromResult<Season?>(null);
  }

  private Task<Episode?> PickMatchingRemoteEpisode(
    Season remoteSeason,
    EpisodeItem episode,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(remoteSeason);
    ArgumentNullException.ThrowIfNull(episode);
    ArgumentNullException.ThrowIfNull(settings);

    if (remoteSeason.Episodes.TryGetValue(episode.Title, out var remoteEpisode))
    {
      return Task.FromResult<Episode?>(remoteEpisode);
    }

    var matches = remoteSeason.Episodes.Values
      .Where(i => episode.Title.FuzzyMatch(i.Title, settings.MaxFuzzyCharacters))
      .ToArray();

    if (matches.Length == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[Red]E[/]: Unable to match [Bold]{episode.Title}[/]");
      return Task.FromResult<Episode?>(null);
    }

    for (; ; )
    {
      var (result, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(int, string display)>()
          .Title($"[Bold]{episode.Title}[/] >>> (?)")
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
        AnsiConsole.MarkupLineInterpolated($"[Yellow]S[/]: Skip [Bold]{episode.Title}[/]");
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
