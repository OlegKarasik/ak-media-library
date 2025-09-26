using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Extensions.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class NormaliseCommand : AsyncCommand<NormaliseCommandSettings>
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
    Item<FilePath> item,
    string directory,
    string name)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentException.ThrowIfNullOrEmpty(name);

    foreach (var file in Directory.EnumerateFiles(item.Path.Directory, $"*{item.Path.Name}.*"))
    {
      File.Move(
        file,
        Path.Combine(
          directory,
          Path.GetFileName(file).Replace(item.Path.Name, name)));
    }
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context,
    NormaliseCommandSettings settings)
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

              var index = await FileServices.LoadAsync(new FilePathIndex(settings.Library));

              foreach (var show in index.Shows)
              {
                foreach (var season in show.Seasons)
                {
                  // Here we need to understand, whether the show has "seasonal" 
                  // structure (i.e. every season is located within a unique, correctly named directory
                  // and all episodes are in these "seasonal" directories.
                  //
                  var normaliseDirectory = false;

                  var directory = season.Path.Value;
                  if (!season.Path.Name.Equals(season.Title))
                  {
                    directory = Path.Combine(show.Path.Value, season.Title);

                    // Here, we create a directory were all episodes
                    // would be moved
                    //
                    Directory.CreateDirectory(directory);

                    normaliseDirectory = true;
                  }

                  foreach (var episode in season.Episodes)
                  {
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

                    var normaliseName = !episode.Path.Name.Equals(name);
                    
                    if (normaliseName || normaliseDirectory)
                    {
                      this.statistics.WriteUpdated();

                      if (normaliseName)
                      {
                        AnsiConsole.MarkupLineInterpolated(
                          $"[[[Yellow]U[/]]]: {episode.Path.Name} [Yellow]->[/] {name}");
                      }
                      if (normaliseDirectory)
                      {
                        AnsiConsole.MarkupLineInterpolated(
                          $"[[[Yellow]U[/]]]: {episode.Path.Name} [Yellow]->[/] {directory}");
                      }

                      RenameItem(episode, directory, name);
                    }
                  }
                }
              }
            });

          if (this.statistics.HasUpdates)
          {
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Updated - [Underline]{this.statistics.Updated}[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
            AnsiConsole.MarkupLineInterpolated($"[[[Red]W[/]]]: The index is [Underline]out of date[/].");
          }
          else
          {
            AnsiConsole.MarkupLineInterpolated($"[[[Green]S[/]]]: Updated - [Underline]None[/], Elapsed - [Underline]{measurement.Elapsed}[/].");
          }
        });

    return 0;
  }
}
