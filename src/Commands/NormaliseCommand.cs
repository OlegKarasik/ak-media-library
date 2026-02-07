using System.Diagnostics;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Commands.Base;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class NormaliseCommand : MediaCommand<NormaliseCommandSettings>
{
  private class NormaliseCommandStatistics
  {
    public bool HasUpdates => this.Updated != 0;

    public long Updated { get; private set; }

    public void WriteUpdated()
    {
      this.Updated += 1;
    }
  }

  private readonly NormaliseCommandStatistics statistics;
  private readonly NormaliseCommandOptions options;

  public NormaliseCommand()
  {
    this.statistics = new NormaliseCommandStatistics();
    this.options = new NormaliseCommandOptions();
  }

  private static void RenameItem(
    Item<DirectoryPath> item,
    string name)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentException.ThrowIfNullOrEmpty(name);

    Directory.Move(item.Path.Value, item.Path.WithName(name).Value);
  }

  private static void RenameItem(
    Item<FilePath> item,
    string name)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentException.ThrowIfNullOrEmpty(name);

    foreach (var file in Directory.EnumerateFiles(item.Path.Directory, $"*{item.Path.Name}.*"))
    {
      File.Move(
        file,
        Path.Combine(
          item.Path.Directory,
          Path.GetFileName(file).Replace(item.Path.Name, name)));
    }
  }

  private static void MoveItem(
    Item<DirectoryPath> item,
    string directory)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentException.ThrowIfNullOrEmpty(directory);

    Directory.Move(item.Path.Value, directory);
  }

  private static void MoveItem(
    Item<FilePath> item,
    string directory)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentException.ThrowIfNullOrEmpty(directory);

    foreach (var file in Directory.EnumerateFiles(item.Path.Directory, $"*{item.Path.Name}.*"))
    {
      File.Move(
        file,
        Path.Combine(
          directory,
          Path.GetFileName(file)));
    }
  }

  private EpisodeItem ProcessEpisode(
    ShowItem show,
    SeasonItem season,
    EpisodeItem episode)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(episode);

    string name;
    if (episode.Position.HasSpan)
    {
      var (Open, Close) = episode.Position.GetSpanPosition();
      name = $"S{episode.Position.GetGroup():D2}E{Open:D2}-E{Close:D2} - {episode.Title}";
    }
    else
    {
      name = $"S{episode.Position.GetGroup():D2}E{episode.Position.GetPosition():D2} - {episode.Title}";
    }

    if (!episode.Path.Name.Equals(name))
    {
      this.statistics.WriteUpdated();

      AnsiConsole.MarkupLineInterpolated(
        $"[[[Yellow]U[/]]]: Renamed {episode.Path.Name} [Yellow]->[/] {name}");

      RenameItem(episode, name);

      return new EpisodeItem
      {
        Title = episode.Title,
        Position = episode.Position,
        Path = new FilePath(Path.Combine(episode.Path.Directory, $"{name}{episode.Path.Extension}"))
      };
    }

    return episode;
  }

  private SeasonItem ProcessSeason(
    ShowItem show,
    SeasonItem season)
  {
    ArgumentNullException.ThrowIfNull(show);
    ArgumentNullException.ThrowIfNull(season);

    string name;
    if (season.Position.GetPosition() == 0)
    {
      name = $"Specials";
    }
    else
    {
      name = $"Season {season.Position.GetPosition()}";
    }

    if (!season.Path.Name.Equals(name))
    {
      var directory = Path.Combine(show.Path.Value, name);
      if (season.Path.Value.Equals(show.Path.Value))
      {
        this.statistics.WriteUpdated();

        AnsiConsole.MarkupLineInterpolated(
          $"[[[Yellow]U[/]]]: Grouped (Season) {season.Path.Name} [Yellow]->[/] {name}");

        // All seasons are located in the "show" directory, therefore 
        // we need to create dedicated show directories and move all episodes
        // there
        //
        Directory.CreateDirectory(directory);

        foreach (var episode in season.Episodes)
        {
          MoveItem(episode, directory);
        }
      }
      else
      {
        this.statistics.WriteUpdated();

        AnsiConsole.MarkupLineInterpolated(
          $"[[[Yellow]U[/]]]: Renamed (Season) {season.Path.Name} [Yellow]->[/] {name}");

        // We need to rename season directory
        //
        RenameItem(season, name);
      }

      // Here, we update the "season" variable to ensure
      // we can proceed with the processing of the episodes
      //
      season = new SeasonItem
      {
        Title = name,
        Position = season.Position,
        Path = new DirectoryPath(directory),
        Episodes = [..
          season.Episodes.Select(episode => new EpisodeItem
          {
            Title = episode.Title,
            Position = episode.Position,
            Path = new FilePath(Path.Combine(directory, $"{episode.Path.Name}{episode.Path.Extension}"))
          })
        ]
      };
    }
    return new SeasonItem
    {
      Title = season.Title,
      Position = season.Position,
      Path = season.Path,
      Episodes = [..Process(show, season)]
    };

    IEnumerable<EpisodeItem> Process(ShowItem show, SeasonItem season)
    {
      foreach (var episode in season.Episodes)
      {
        yield return this.ProcessEpisode(show, season, episode);
      }
    }
  }

  private ShowItem ProcessShow(
    ShowItem show)
  {
    ArgumentNullException.ThrowIfNull(show);
    
    return new ShowItem
    {
      Title = show.Title,
      Path = show.Path,
      Seasons = [.. Process(show)]
    };

    IEnumerable<SeasonItem> Process(ShowItem show)
    {
      foreach (var season in show.Seasons)
      {
        yield return this.ProcessSeason(show, season);
      }
    }
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context,
    NormaliseCommandSettings settings,
    CancellationToken cancellationToken)
  {
    await AnsiConsole
      .Status()
      .StartAsync(
        "Initialising...",
        async ctx =>
        {
          var measurement = await TimeServices.MeasureAsync(
            async () =>
            {
              ctx.Status("Normalising...");

              var index = await GetAsync(new FilePathIndex(settings.Library));
              index = new IndexItem
              {
                Path = index.Path,
                Movies = index.Movies,
                Shows = [.. Process(index.Shows)]
              };

              await SaveAsync(index, index.Path);
            });

          if (this.statistics.HasUpdates)
          {
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Updated - [Underline]{this.statistics.Updated}[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
            AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: The index is [Underline]updated[/].");
          }
          else
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Updated - [Underline]None[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
          }
        });

    IEnumerable<ShowItem> Process(IEnumerable<ShowItem> shows)
    {
      foreach (var show in shows)
      {
        yield return this.ProcessShow(show);
      }
    }

    return 0;
  }
}
