using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business.Properties;
using MediaLibrary.Business;
using System.Text.RegularExpressions;

namespace MediaLibrary.Commands;

public abstract class Item
{
}

public class IgnoreItem : Item
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

public enum FileType
{
  Video,
  Subtitles,
  Image
}

public class ScanLibraryCommand : AsyncCommand<ScanLibraryCommandSettings>
{
  private static HashSet<string> extensions = [".mp3", ".mp4", ".avi", ".mkv"];

  private static string[] episodeRegex = new []
  {
    @"^S(?<season>\d+)E(?<episode>\d+)\s+-?\s+(?<title>.+)$"
  };
  private static string[] ignoreRegex = new []
  {
    @"^\._"
  };

  private static Item Scan(
    FilePath path)
  {
    // We focus on recognisable video files
    //
    if (!extensions.Contains(path.FileExtension))
    {
      return new IgnoreItem();
    }
    foreach (var regex in ignoreRegex)
    {
      if (Regex.IsMatch(path.FileName, regex))
      {
        return new IgnoreItem();
      }
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
    var directories = Directory.EnumerateDirectories(path.Value)
      .Select(j => Scan(new DirectoryPath(j)))
      .Where(j => j is not EmptyItem && j is not IgnoreItem)
      .ToArray();

    var files = Directory.EnumerateFiles(path.Value)
      .Select(j => Scan(new FilePath(j)))
      .Where(j => j is not EmptyItem && j is not IgnoreItem)
      .ToArray();

    if (directories.Any() && files.Any())
    {
      throw new Exception();
    }
    if (directories.Any())
    {
      if (directories.All(i => i is SeasonItem))
      {
        return new ShowItem();
      }
      if (directories.All(i => i is ShowItem))
      {
        return new ShowLibraryItem();
      }
      if (directories.All(i => i is MovieLibraryItem))
      {
        return new MovieLibraryItem();
      }
      throw new Exception();
    }
    if (files.Any())
    {
      if (files.All(i => i is EpisodeItem))
      {
        return new SeasonItem();
      }
      if (files.All(i => i is MovieItem))
      {
        return new MovieLibraryItem();
      }
      throw new Exception();
    }
    return new EmptyItem();
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