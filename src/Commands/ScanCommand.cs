using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;

namespace MediaLibrary.Commands;

public abstract class FileItem
{
  public required FilePath Path 
  { 
    get; init; 
  }
}

public abstract class DirectoryItem
{
  public required DirectoryPath Path 
  { 
    get; init;
  }
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
  public required EpisodeItem[] Episodes 
  { 
    get; init; 
  }
}

public class ShowItem : DirectoryItem
{
  public required SeasonItem[] Seasons 
  { 
    get; init; 
  }
}

public class LibraryItem : DirectoryItem
{
  public required MovieItem[] Movies 
  { 
    get; init; 
  }

  public required ShowItem[] Shows 
  { 
    get; init; 
  }
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
          seasons.Add(season);
          break;
        case ShowItem show:
          shows.Add(show);
          break;
        case LibraryItem library:
          libraries.Add(library);
          break;
        default:
          // We don't really care about empty items
          //
          continue;
      }
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

    if (seasons.Count != 0)
    {
      // When we have seasons, we can have seasons
      //
      if (movies.Count != 0 || episodes.Count != 0 || libraries.Count != 0 || shows.Count != 0)
      {
        throw new NotSupportedException();
      }
      return new ShowItem
        {
          Path = path,
          Seasons = [ .. seasons ]
        };
    }
    if (libraries.Count != 0 || shows.Count != 0)
    {
      // When we have libraries or shows, we can have libraries, shows or movies
      //
      if (episodes.Count != 0)
      {
        throw new NotSupportedException();
      }
      return new LibraryItem
        {
          Path = path,
          Shows = [ .. libraries.SelectMany(i => i.Shows), .. shows ],
          Movies = [ .. libraries.SelectMany(i => i.Movies), .. movies ]
        };
    }

    // If we have a directory with both episodes then we don't know what to do
    //
    if (episodes.Count != 0 && movies.Count != 0)
    {
      throw new NotSupportedException();
    }

    // If we have episodes, then it is a season
    //
    if (episodes.Count != 0)
    {
      return new SeasonItem() 
        { 
          Path = path, 
          Episodes = [ .. episodes ] 
        };
    }

    // If we have movies, then it is a library
    //
    if (movies.Count != 0)
    {
      return new LibraryItem() 
        { 
          Path = path,
          Movies = [ .. movies ],
          Shows = []
        };
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
          var item = this.Scan(new DirectoryPath(settings.LibraryPath));

          ctx.Status("Saving...");  

          return Task.CompletedTask;
        });
        

    return 0;
  }
}