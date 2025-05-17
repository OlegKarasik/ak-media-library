using MediaLibrary.Business;
using MediaLibrary.Business.Enrichment.Models;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions;
using MediaLibrary.Extensions.Services;
using MediaLibrary.Extensions.Services.InterfaceContrls;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public partial class NormaliseCommand : AsyncCommand<NormaliseCommandSettings>
{
  private readonly NormaliseCommandOptions options;

  public NormaliseCommand()
  {
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

              foreach (var show in index.Shows.Values)
              {
                foreach (var season in show.Seasons.Values)
                {
                  foreach (var episode in season.Episodes.Values)
                  {
                    var t = new Title(episode.Title);
                    if (!episode.Title.Equals(t.ToString()))
                    {
                      var name = episode.Path.Name.Replace(
                        episode.Title,
                        t.ToString());

                      //FileServices.RenameGroup(episode.Path, name);
                    }
                  }
                }
              }
            });

            AnsiConsole.MarkupLineInterpolated($"Normalisation completed [Green]Elapsed: {measurement.Elapsed}[/]. Please execute \"[Yellow]scan[/]\" command to update the index.");
        });

    return 0;
  }
}
