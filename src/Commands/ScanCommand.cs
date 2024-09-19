using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;

namespace MediaLibrary.Commands;

public abstract class FileItem
{
  public required FilePath Path { get; init; }
}

public abstract class DirectoryItem
{
  public required DirectoryPath Path { get; init; }
}

public class IgnoreItem : FileItem
{
}

public class UnknownItem : FileItem
{
}

public class EpisodeItem : FileItem
{
}

public class MovieItem : FileItem
{
}

public class EmptyItem : DirectoryItem
{
}

public class SeasonItem : DirectoryItem
{
}

public class ShowItem : DirectoryItem
{
}

public class LibraryItem : DirectoryItem
{
}

public class ScanCommand : AsyncCommand<ScanCommandSettings>
{
  private readonly ScanCommandOptions options;

  public ScanCommand()
  {
    this.options = new ScanCommandOptions();
  }

  private FileItem Scan(
    FilePath path)
  {
    // Rule out all unsupported files
    //
    if (!this.options.FileExtensions.Contains(path.FileExtension))
    {
      return new IgnoreItem { Path = path };
    }
    foreach (var pattern in this.options.FileIgnorePatterns)
    {
      if (Regex.IsMatch(path.FileName, pattern))
      {
        return new IgnoreItem { Path = path };
      }
    }

    // Attempt to match the file by one of the episode patterns
    //
    foreach (var pattern in this.options.EpisodeMatchPatterns)
    {
      var match = Regex.Match(path.FileName, pattern);
      if (match.Success)
      {
        return new EpisodeItem 
          { 
            Path = path 
          };
      }
    }

    // Attempt to match the file by one of the movie patterns
    //
    foreach (var pattern in this.options.MovieMatchPatterns)
    {
      var match = Regex.Match(path.FileName, pattern);
      if (match.Success)
      {
        return new MovieItem
          {
            Path = path
          };
      }
    }

    // Return unknown items
    //
    return new UnknownItem { Path = path };
  }

  private DirectoryItem Scan(
    DirectoryPath path)
  {
    List<SeasonItem> seasons = [];
    List<ShowItem> shows = [];
    List<LibraryItem> libraries = [];
    foreach (var directoryPath in Directory.EnumerateDirectories(path.Value))
    {
      var directoryItem = this.Scan(new DirectoryPath(directoryPath));
      switch (directoryItem)
      {
        case SeasonItem season:
          AnsiConsole.MarkupLine($"Season: {season.Path}");
          seasons.Add(season);
          break;
        case ShowItem show:
          AnsiConsole.MarkupLine($"Show: {show.Path}");
          shows.Add(show);
          break;
        case LibraryItem library:
          AnsiConsole.MarkupLine($"Library: {library.Path}");
          libraries.Add(library);
          break;
        default:
          // We don't really care about empty items
          //
          continue;
      }
    }

    // If directory contains libraries or shows, then we consider
    // it to be a library
    //
    if (libraries.Count != 0 || shows.Count != 0)
    {
      return new LibraryItem() { Path = path };
    }

    // If directory contains seasons, then we consider 
    // it to be a show
    //
    if (seasons.Count != 0)
    {
      return new ShowItem() { Path = path };
    }

    List<EpisodeItem> episodes = [];
    List<MovieItem> movies = [];
    foreach (var filePath in Directory.EnumerateFiles(path.Value))
    {
      var fileItem = this.Scan(new FilePath(filePath));
      switch (fileItem)
      {
        case EpisodeItem episode:
          episodes.Add(episode);
          break;
        case MovieItem movie:
          movies.Add(movie);
          break;
        case UnknownItem unknown:
          throw new NotImplementedException();
        default:
          // We don't really care about ignore items
          //
          continue;
      }
    }

    // If we have a directory with both episodes and
    // movies then we throw NotSupportedException because
    // we don't know what to do
    //
    if (episodes.Count != 0 && movies.Count != 0)
    {
      throw new NotSupportedException();
    }

    // If we have episodes, then it is a season
    //
    if (episodes.Count != 0)
    {
      return new SeasonItem() { Path = path };
    }

    // If we have movies, then it is a library
    //
    if (movies.Count != 0)
    {
      return new LibraryItem() { Path = path };
    }
    return new EmptyItem() { Path = path };
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    ScanCommandSettings settings)
  {
    await AnsiConsole
      .Status()
      .StartAsync(
        "Scanning...", 
        ctx => 
        {
          this.Scan(new DirectoryPath(settings.LibraryPath));

          return Task.CompletedTask;
        });

    return 0;
  }
}