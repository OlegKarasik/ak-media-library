using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business.Properties;
using MediaLibrary.Business;
using System.Text.RegularExpressions;

namespace MediaLibrary.Commands;

public abstract class Item
{
}

public class EmptyItem : Item
{
}

public class MovieItem : Item
{
}

public class EpisodeItem : Item
{
}

public class SeasonItem : Item
{
}

public class ShowItem : Item
{
}

public class MovieLibraryItem : Item
{
}

public class ShowLibraryItem : Item
{
}

public class ScanLibraryCommand : AsyncCommand<ScanLibraryCommandSettings>
{
  private static string[] episodeRegex = new []
  {
    @"^S(?<season>\d+)E(?<episode>\d+)\s+-?\s+(?<title>.+)$"
  };

  private static Item Scan(
    FilePath path)
  {
    if (PropsPath.IsPropsPath(path))
    {
      // We ignore props paths by themselves
      //
      return new EmptyItem();
    }
    // The path we are scanning is a file, therefore we need to understand
    // whether it is a movie or an episode
    //
    foreach (var regex in episodeRegex)
    {
      var match = Regex.Match(path.FileName, regex);
      if (match.Success)
      {
        // Return episode data
        //
        return new EpisodeItem();
      }
    }
    // Return movie data
    //
    return new MovieItem();
  }

  private static Item Scan(
    DirectoryPath path)
  {
    foreach (var i in Directory.EnumerateDirectories(path.Value).Select(j => new DirectoryPath(j)))
    {
      Scan(i);
    }
    foreach (var i in Directory.EnumerateFiles(path.Value).Select(j => new FilePath(j)))
    {
      Scan(i);
    }
    return null;
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    ScanLibraryCommandSettings settings)
  {
    await AnsiConsole
      .Status()
      .StartAsync(
        "Scanning...", 
        async ctx => 
        {
          Scan(new DirectoryPath(settings.LibraryPath));
        });

    return 0;
  }
}