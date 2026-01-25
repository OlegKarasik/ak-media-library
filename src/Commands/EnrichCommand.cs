using System.Reflection.Metadata.Ecma335;
using MediaLibrary.Business;
using MediaLibrary.Business.Enrichment;
using MediaLibrary.Business.Enrichment.Models;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Commands.Base;
using MediaLibrary.Extensions;
using MediaLibrary.Extensions.Services;
using MediaLibrary.Extensions.Services.InterfaceContrls;
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

  public async Task<Search?> PickMatch(
    ShowItem show,
    EnrichCommandSettings settings)
  {
    var measure = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync("Searching", async ctx => await this.enrichment.SearchSeriesAsync(show.Title, settings.Language)));

    for (;;)
    {
      var (pickResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(int, string display)>()
          .Title("Pick the show to view details")
          .UseConverter(item => item.display)
          .AddChoices(
            [.. 
              measure.Data.Select((item, i) => (index: i, value: $"{item.Title} ({item.Year})")), 
              (-1, "Cancel")
            ]
          ));

      if (pickResult == -1)
      {
        return null;  
      }

      var match = measure.Data[pickResult];

      AnsiConsole.Write(
        new Panel(new Text(match.Overview)).Header($"{match.Title}:U", Justify.Left));

      var (confirmationResult, _) = AnsiConsole.Prompt(
        new SelectionPrompt<(bool, string display)>()
          .Title("Do you want to proceed with the show?")
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

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    EnrichCommandSettings settings)
  {
    var index = await GetAsync(new FilePathIndex(settings.Library));
    switch (IndexSearch.GetItem(index, settings.SearchRequest)) 
    {
      case ShowItem show:
        {
          var result = await this.PickMatch(show, settings);
          var remoteSearch = await this.SearchShowsAsync(show.Title, settings);
          if (remoteSearch.Length == 0)
          {
            return -1;
          }

          var remoteShow = await this.GetSeriesAsync(this.PickSearch(remoteSearch), settings);

          await this.EnrichAsync(show, remoteShow);

          AnsiConsoleService.Rule(show.Title);

          foreach (var season in show.Seasons)
          {
            if (!remoteShow.Seasons.TryGetValue((long)season.Position.GetPosition(), out var remoteSeason))
            {
              continue;
            }
            await this.EnrichAsync(season, remoteSeason);

            AnsiConsoleService.Rule($"Season {remoteSeason.Index}");

            foreach (var episode in season.Episodes)
            {
              if (!remoteSeason.Episodes.TryGetValue(episode.Title, out var remoteEpisode))
              {
                remoteEpisode = this.PickEpisode(episode.Title, remoteSeason.Episodes.Values, settings);
                if (remoteEpisode is null)
                {
                  continue;
                }
              }
              await this.EnrichAsync(episode, remoteEpisode);
            }
          }
        }
        break;
      default:
        throw new InvalidOperationException($"The '{settings.SearchRequest}' isn't found in index");
    }

    return 0;
  }

  private async Task<Search[]> SearchShowsAsync(
    string title,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(title);
    ArgumentNullException.ThrowIfNull(settings);

    var measure = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync("Searching", async ctx => await this.enrichment.SearchSeriesAsync(title, settings.Language)));

    AnsiConsole.MarkupLineInterpolated($"Found [Green]{measure.Data.Length}[/] matching shows. Elapsed [Green]{measure.Elapsed}[/]");

    return measure.Data;
  }

  private async Task<Series> GetSeriesAsync(
    Search search,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(search);
    ArgumentNullException.ThrowIfNull(settings);

    var measure = await TimeServices.MeasureAsync(
      async () => 
        await AnsiConsole
          .Status()
          .StartAsync($"Downloading [Green]{search.Title}[/]", async ctx => await this.enrichment.GetSeriesAsync(search.Id, settings.Language)));

    AnsiConsole.MarkupLineInterpolated($"Downloaded information about [Green]{search.Title}[/] series. Elapsed [Green]{measure.Elapsed}[/]");

    return measure.Data;
  }

  private Search PickSearch(
    IEnumerable<Search> search)
  {
    ArgumentNullException.ThrowIfNull(search);

    for (;;)
    {
      var value = AnsiConsoleService.SelectOneOf(search, i => $"{i.Title} ({i.Year})");
      Print(value);

      if (!AnsiConsoleService.SelectYesOrNo())
      {
        continue;
      }
      
      return value;
    }
  }

  private Episode? PickEpisode(
    EpisodeTitle title,
    IEnumerable<Episode> episodes,
    EnrichCommandSettings settings)
  {
    ArgumentNullException.ThrowIfNull(title);
    ArgumentNullException.ThrowIfNull(episodes);
    ArgumentNullException.ThrowIfNull(settings);

    var matches = new List<Episode>();
    foreach (var episode in episodes)
    {
      if (title.ToString().CalculateLevenshteinDistance(episode.Title.ToString()) < settings.FuzzyMatch)
      {
        matches.Add(episode);
      }
    }
    if (matches.Count == 0)
    {
      AnsiConsole.MarkupLineInterpolated($"[Red]FAILED[/] to match remote episodes to [Green]{title}[/]");
      return null;
    }

    var selectPrompt = new PromptSelectControl<Episode>(
        $"Found [Underline]{matches.Count}[/] episode(s) matching [Bold]\"{title}\"[/]",
        matches,
        [PromptCommands.Skip]
      )
      .UseItemString(i => $"{i.Title} (Season {i.SeasonIndex}, {i.Date})");

    var updatePrompt = new PromptSelectControl<bool>(
        "Update metadata?",
        [],
        [PromptCommands.Yes, PromptCommands.No]);

    for (; ; )
    {
      switch (AnsiConsole.Prompt(selectPrompt.GetPrompt()))
      {
        case PromptSelectControl<Episode>.PromptItemResult result:
          AnsiConsole.Write(new VisualPanelControl(result.Item.Title.ToString(), result.Item.Overview).GetRenderable());

          switch (AnsiConsole.Prompt(updatePrompt.GetPrompt()).Match)
          {
            case PromptSelectControl<bool>.PromptMatches.Yes:
              return result.Item;
            case PromptSelectControl<bool>.PromptMatches.No:
              continue;
            default:
              throw new NotImplementedException();
          }
        case PromptSelectControl<Episode>.PromptControlResult control:
          switch (control.Match)
          {
            case PromptSelectControl<Episode>.PromptMatches.Skip:
              AnsiConsole.MarkupLineInterpolated($"[YELLOW]SKIPPED[/] matching of [Green]{title}[/] episode");
              return null;
            default:
              throw new NotImplementedException();
          }
      }
    }
  }

  private async Task EnrichAsync(
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
    
  private async Task EnrichAsync(
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

  private async Task EnrichAsync(
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
  
  public static void Print(
    Search remoteSearch)
  {
    ArgumentNullException.ThrowIfNull(remoteSearch);

    AnsiConsole.Write(
      new Rows(
        new Text(string.Empty),
        new Panel(new Text(remoteSearch.Overview))
          .Header(remoteSearch.Title.ToUpper(), Justify.Left)));
  }
}
