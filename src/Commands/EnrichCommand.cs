using MediaLibrary.Business;
using MediaLibrary.Business.Enrichment;
using MediaLibrary.Business.Enrichment.Models;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Commands.Base;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class EnrichCommand : MediaCommand<EnrichCommandSettings>
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
    var index = await GetAsync(new FilePathIndex(settings.Library));
    switch (IndexSearch.GetItem(index, settings.SearchRequest))
    {
      case ShowItem show:
        {
          var remoteSeries = await this.PickMatchingRemoteSeries(show, settings);
          if (remoteSeries is not null)
          {
            await this.EnrichShowItemAsync(show, remoteSeries);

            foreach (var season in show.Seasons)
            {
              var remoteSeason = await this.PickMatchingRemoteSeason(remoteSeries, season, settings);
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
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }

  private async Task<Series?> PickMatchingRemoteSeries(
    ShowItem show,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(settings);

    var remoteSearch =
      await AnsiConsole
        .Status()
        .StartAsync("Searching", async ctx => await this.enrichment.SearchSeriesAsync(show.Title, settings.Language));

    for (; ; )
    {
      var (pickResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(int, string display)>()
          .Title("Pick the show to see more details")
          .UseConverter(item => item.display)
          .AddChoices(
            [..
              remoteSearch.Select((item, i) => (index: i, value: $"{item.Title} ({item.Year})")),
              (-1, "[CANCEL]")
            ]
          ));

      // Cancel
      //
      if (pickResult == -1)
      {
        return null;
      }

      var match = remoteSearch[pickResult];

      AnsiConsole.Write(
        new Panel(new Text(match.Overview)).Header($"{match.Title}:U", Justify.Left));

      var (confirmationResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(bool, string display)>()
          .Title("Do you want to use this show?")
          .UseConverter(item => item.display)
          .AddChoices(
            [
              (true, "Yes"), (false, "No")
            ]
          ));

      if (confirmationResult)
      {
        var remoteSeries =
          await AnsiConsole
            .Status()
            .StartAsync($"Downloading", async ctx => await this.enrichment.GetSeriesAsync(match.Id, settings.Language));

        return remoteSeries;
      }
    }
  }

  private async Task<Season?> PickMatchingRemoteSeason(
    Series remoteSeries,
    SeasonItem season,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(remoteSeries);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(settings);
    
    return remoteSeries.Seasons.TryGetValue((long)season.Position.GetPosition(), out var remoteSeason)
      ? remoteSeason
      : null;
  }

  private async Task<Episode?> PickMatchingRemoteEpisode(
    Season remoteSeason,
    EpisodeItem episode,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(remoteSeason);
    ArgumentNullException.ThrowIfNull(episode);
    ArgumentNullException.ThrowIfNull(settings);

    if (remoteSeason.Episodes.TryGetValue(episode.Title, out var remoteEpisode))
    {
      return remoteEpisode;
    }

    var matches = remoteSeason.Episodes.Values
      .Where(i => episode.Title.FuzzyMatch(i.Title, settings.FuzzyMatch))
      .ToArray();

    for (; ; )
    {
      var (pickResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(int, string display)>()
          .Title("Pick the episode to see more details")
          .UseConverter(item => item.display)
          .AddChoices(
            [..
              matches.Select((item, index) => (index, $"{item.Title} (Season {item.SeasonIndex}, {item.Date})")),
              (-1, "[SKIP]")
            ]
          ));

      // Skip
      //
      if (pickResult == -1)
      {
        return null;
      }

      var match = matches[pickResult];

      AnsiConsole.Write(
        new Panel(new Text(match.Overview)).Header($"{match.Title}:U", Justify.Left));

      var (confirmationResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(bool, string display)>()
          .Title("Do you want to use this episode?")
          .UseConverter(item => item.display)
          .AddChoices(
            [
              (true, "Yes"), (false, "No")
            ]
          ));

      if (confirmationResult)
      {
        return match;
      }
    }
  }

  private async Task EnrichShowItemAsync(
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

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]{show.Title}[/]");
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

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]Season {season.Position.GetPosition()}[/]");
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

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]{episode.Title}[/]");
  }
}
