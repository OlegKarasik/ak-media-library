using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Matches;

namespace MediaLibrary.Commands;

public class ScanCommand : AsyncCommand<ScanCommandSettings>
{
  private readonly ScanCommandOptions options;

  public ScanCommand()
  {
    this.options = new ScanCommandOptions();
  }

  private FileItem ScanFile(
    FilePath path)
  {
    // Rule out all unsupported files
    //
    if (!this.options.FileExtensions.Contains(path.Extension))
    {
      return new IgnoreItem { Path = path };
    }
    foreach (var pattern in this.options.FileIgnorePatterns)
    {
      if (Regex.IsMatch(path.Name, pattern))
      {
        return new IgnoreItem { Path = path };
      }
    }

    // Attempt to match the file by one of the episode patterns
    //
    foreach (var pattern in this.options.EpisodeMatchPatterns)
    {
      var match = Regex.Match(path.Name, pattern);
      if (match.Success)
      {
        var m = new EpisodeItemMatch(match);
        return new EpisodeItem 
          { 
            Title = m.Title ?? throw new Exception("The episode Regex match must include (?<title>) group"),
            Position = m.Position ?? throw new Exception("The episode Regex match must include (?<episode>) group"),
            Path = path 
          };
      }
    }

    // Attempt to match the file by one of the movie patterns
    //
    foreach (var pattern in this.options.MovieMatchPatterns)
    {
      var match = Regex.Match(path.Name, pattern);
      if (match.Success)
      {
        var m = new MovieItemMatch(match);
        return new MovieItem
          {
            Title = m.Title ?? throw new Exception("The movie Regex match must include (?<title>) group"),
            Path = path
          };
      }
    }
    throw new NotImplementedException();
  }

  private DirectoryItem ScanDirectory(
    DirectoryPath path)
  {
    List<SeasonItem> seasons = [];
    List<ShowItem> shows = [];
    List<LibraryItem> libraries = [];
    foreach (var directoryPath in Directory.EnumerateDirectories(path.Value))
    {
      var directoryItem = this.ScanDirectory(new DirectoryPath(directoryPath));
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
        case EmptyItem:
          break;
        default:
          throw new NotImplementedException();
      }
    }

    List<EpisodeItem> episodes = [];
    List<MovieItem> movies = [];
    foreach (var filePath in Directory.EnumerateFiles(path.Value))
    {
      var fileItem = this.ScanFile(new FilePath(filePath));
      switch (fileItem)
      {
        case EpisodeItem episode:
          episodes.Add(episode);
          break;
        case MovieItem movie:
          movies.Add(movie);
          break;
        case IgnoreItem:
          break;
        default:
          throw new NotImplementedException();
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
      foreach (var pattern in this.options.ShowMatchPatterns)
      {
        var match = Regex.Match(path.Name, pattern);
        if (match.Success)
        {
          return new ShowItem
            {
              Title = match.Groups["title"].Value,
              Path = path,
              Seasons = [ .. seasons ]
            };
        }
      }
      throw new NotImplementedException();
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
      foreach (var pattern in this.options.SeasonMatchPatterns)
      {
        var match = Regex.Match(path.Name, pattern);
        if (match.Success)
        {
          return new SeasonItem() 
            { 
              Title = match.Groups["title"].Value,
              Path = path, 
              Episodes = [ .. episodes ] 
            };
        }
      }
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

  private LibraryItem ScanLibrary(
    DirectoryPath path)
  {
    var item = this.ScanDirectory(path);
    if (item is LibraryItem library)
    {
      return library;
    }
    throw new NotSupportedException();
  }

  public override Task<int> ExecuteAsync(
    CommandContext context, 
    ScanCommandSettings settings)
  {
    var library = AnsiConsole
      .Status()
      .Start("Scanning...", ctx => this.ScanLibrary(new DirectoryPath(settings.LibraryPath)));
        
    

    return Task.FromResult(0);
  }
}