using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions;
using MediaLibrary.Extensions.Services;
using MediaLibrary.Extensions.Services.Enrichment.Models;
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
    var index = await AnsiConsole
      .Status()
      .StartAsync("Loading index", async ctx => await FileServices.LoadAsync(new DirectoryIndexFilePath(settings.Library.Value)));

    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case ShowItem show:
        {
          var _show = await this.PickShowAsync(show.Title, settings);
          if (_show is null)
          {
            return -1;
          }
          await this.EnrichShowAsync(show, _show);

          AnsiConsoleService.Rule(_show.Title);

          foreach (var season in show.Seasons.Values)
          {
            if (!_show.Seasons.TryGetValue((long)season.Position.GetPosition(), out var _season))
            {
              continue;
            }
            await this.EnrichSeasonAsync(season, _season);

            AnsiConsoleService.Rule($"Season {_season.Index}");

            foreach (var episode in season.Episodes.Values)
            {
              var _episode = await this.PickEpisodeMatchAsync(episode.Title, _season.Episodes, settings);
              if (_episode is null)
              {
                continue;
              }
              await this.EnrichEpisodeAsync(episode, _episode);
            }
          }
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }

  private async Task<Series?> PickShowAsync(
    string title,
    EnrichCommandSettings settings)
  {
    var measurementSearch = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync("Searching", async ctx => await this.enrichment.SearchSeriesAsync(title, settings.Language)));

    if (measurementSearch.Data is null)
    {
      AnsiConsole.MarkupLineInterpolated($"No shows matching [Green]{title}[/] found. Elapsed [Green]{measurementSearch.Elapsed}[/]");
      return null;
    }

    AnsiConsole.MarkupLineInterpolated($"Found [Green]{measurementSearch.Data.Length}[/] matching shows. Elapsed [Green]{measurementSearch.Elapsed}[/]");

    if (!AnsiConsoleService.Question("Continue?"))
    {
      return null;
    }
    
    for (;;)
    {
      var pick = AnsiConsoleService.Select(measurementSearch.Data, i => $"{i.Name} ({i.Year})");
      Print(pick);

      if (!AnsiConsoleService.Question("Continue?"))
      {
        continue;
      }
      
      var measurementSeries = await TimeServices.MeasureAsync(
        async () => 
          await AnsiConsole
            .Status()
            .StartAsync($"Downloading [Green]{title}[/]", async ctx => await this.enrichment.GetSeriesAsync(pick.Id, settings.Language)));

      AnsiConsole.MarkupLineInterpolated($"Downloaded information about [Green]{title}[/] series. Elapsed [Green]{measurementSearch.Elapsed}[/]");

      return measurementSeries.Data;
    }
  }

  private async Task<Episode?> PickEpisodeMatchAsync(
    string title,
    Dictionary<string, Episode> episodes,
    EnrichCommandSettings settings)
  {
    if (episodes.TryGetValue(title, out var match))
    {
      return match;
    }

    var fuzzy = new List<Episode>();
    foreach (var episode in episodes.Values)
    {
      if (title.CalculateLevenshteinDistance(episode.Title) < settings.FuzzyMatch)
      {
        fuzzy.Add(episode);
      }
    }
    if (fuzzy.Count == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[Red]FAILED[/] to match remote episodes to [Green]{title}[/]");
      return null;
    }

    AnsiConsole.MarkupLineInterpolated($"Found [Green]{fuzzy.Count}[/] episodes which more or less match [Red]{title}[/]");

    for (;;)
    {
      match = AnsiConsoleService.Select(fuzzy, i => $"{i.Title} (Season {i.SeasonIndex}, {i.Date})");
      Print(match);

      switch (AnsiConsoleService.SelectContinueBackSkip())
      {
        case AnsiConsoleService.ContinueBackSkip.Skip:
          match = null;

          AnsiConsole.MarkupLineInterpolated($"[YELLOW]SKIPPED[/] matching of [Green]{title}[/] episode");
          break;
        case AnsiConsoleService.ContinueBackSkip.Back:
          continue;
        default:
          break;
      }
      break;
    }
    return null;
  }

  private async Task EnrichShowAsync(
    ShowItem show,
    Series _show)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync(
            $"Enriching [Green]{show.Title}[/]",
            async ctx => 
            {
              if (_show.Image is not null)
              {
                await FileServices.SaveAsync(
                  _show.Image,
                  new DirectoryImageFilePath(show.Path.Value));
              }
              if (_show.ImageBackground is not null)
              {
                await FileServices.SaveAsync(
                  _show.ImageBackground,
                  new DirectoryImageBackgroundFilePath(show.Path.Value));
              }

              await FileServices.SaveAsync(
                new ShowPropsItem
                {
                  Summary = [_show.Overview ??  string.Empty],
                  Date = _show.Date,
                  Genres = _show.Genres
                }, 
                new DirectoryPropsFilePath(show.Path.Value));
            }));

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]{show.Title}[/]. Elapsed [Green]{measurement.Elapsed}[/]");
  }
    
  private async Task EnrichSeasonAsync(
    SeasonItem season,
    Season _season)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync(
            $"Enriching [Green]Season {season.Position.GetPosition()}[/]",
            async ctx => 
            {
              if (_season.Image is not null)
              {
                await FileServices.SaveAsync(
                  _season.Image,
                  new DirectoryImageFilePath(season.Path.Value));
              }
              if (_season.ImageBackground is not null)
              {
                await FileServices.SaveAsync(
                  _season.ImageBackground,
                  new DirectoryImageBackgroundFilePath(season.Path.Value));
              }

              await FileServices.SaveAsync(
                new SeasonPropsItem
                {
                  Summary = [_season.Overview ?? string.Empty],
                }, 
                new DirectoryPropsFilePath(season.Path.Value));
            }));

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]Season {season.Position.GetPosition()}[/]. Elapsed [Green]{measurement.Elapsed}[/]");
  }

  private async Task EnrichEpisodeAsync(
    EpisodeItem episode,
    Episode _episode)
  {
    var measurement = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync(
            $"Enriching [Green]{episode.Title}[/]",
            async ctx => 
            {
              await FileServices.SaveAsync(
                new EpisodePropsItem
                {
                  Date = _episode.Date,
                  Summary = [_episode.Overview ?? string.Empty],
                  Directors = [.. _episode.Directors.Select(i => i.Name)],
                  Writers = [.. _episode.Writers.Select(i => i.Name)],
                }, 
                new FilePropsFilePath(episode.Path.Value));
            }));

    AnsiConsole.MarkupLineInterpolated($"Enriched [Green]{episode.Title}[/]. Elapsed [Green]{measurement.Elapsed}[/]");
  }
  
  public static void Print(
    Search search)
  {
    if (search is null)
    {
      throw new ArgumentNullException(nameof(search));
    }

    AnsiConsole.Write(
      new Rows(
        new Text(string.Empty),
        new Panel(new Text(search.Overview ?? string.Empty))
          .Header(search.Name.ToUpper(), Justify.Left)));
  }

  public static void Print(
    Episode episode)
  {
    if (episode is null)
    {
      throw new ArgumentNullException(nameof(episode));
    }

    AnsiConsole.Write(
      new Rows(
        new Text(string.Empty),
        new Panel(new Text(episode.Overview ?? string.Empty))
          .Header(episode.Title.ToUpper(), Justify.Left)));
  }
}
