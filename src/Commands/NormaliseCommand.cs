using MediaLibrary.Business;
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
                  foreach (var episode in season.Episodes)
                  {
                    string result;
                    if (episode.Position.HasSpan)
                    {
                      var (Open, Close) = episode.Position.GetSpanPosition();
                      result = $"S{episode.Position.GetGroup():D2}E{Open:D2}-E{Close:D2} - {episode.Title}";
                    }
                    else
                    {
                      result = $"S{episode.Position.GetGroup():D2}E{episode.Position.GetPosition():D2} - {episode.Title}";
                    }

                    if (!episode.Path.Name.Equals(result))
                    {
                      this.statistics.WriteUpdated();

                      AnsiConsole.MarkupLineInterpolated($"[[[Yellow]U[/]]]: {episode.Path.Name} [Yellow]->[/] {result}");
                      // var name1 = episode.Path.Name.Replace(
                      //   episode.Title,
                      //   t.ToString());

                      //FileServices.RenameGroup(episode.Path, name);
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
