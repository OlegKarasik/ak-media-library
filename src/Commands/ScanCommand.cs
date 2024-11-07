using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using System.Text.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace MediaLibrary.Commands;

public class ScanCommand : AsyncCommand<ScanCommandSettings>
{
  [Flags]
  public enum ScanItemMask
  {
    None      = 0b00000000,
    Episodes  = 0b00000001,
    Movies    = 0b00000010,
    Seasons   = 0b00000100,
    Shows     = 0b00001000,
    Libraries = 0b00010000
  }

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
      return new IgnoreItem 
        { 
          Path = path 
        };
    }
    foreach (var pattern in this.options.FileIgnorePatterns)
    {
      if (Regex.IsMatch(path.Name, pattern))
      {
        return new IgnoreItem 
          { 
            Path = path 
          };
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
            Title = m.Title,
            Index = m.Index,
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
            Title = m.Title,
            Path = path
          };
      }
    }
    throw new NotImplementedException();
  }

  private DirectoryItem ScanDirectory(
    DirectoryPath path)
  {
    var mask = ScanItemMask.None;

    List<SeasonItem> seasons = [];
    List<ShowItem> shows = [];
    List<LibraryItem> libraries = [];
    foreach (var directoryPath in Directory.EnumerateDirectories(path.Value))
    {
      var directoryItem = this.ScanDirectory(new DirectoryPath(directoryPath));
      switch (directoryItem)
      {
        case SeasonItem season:
          if (seasons.Count == 0)
          {
            mask |= ScanItemMask.Seasons;
          }
          seasons.Add(season);
          break;
        case ShowItem show:
          if (shows.Count == 0)
          {
            mask |= ScanItemMask.Shows;
          }
          shows.Add(show);
          break;
        case LibraryItem library:
          if (libraries.Count == 0)
          {
            mask |= ScanItemMask.Libraries;
          }
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
          if (episodes.Count == 0)
          {
            mask |= ScanItemMask.Episodes;
          }
          episodes.Add(episode);
          break;
        case MovieItem movie:
          if (movies.Count == 0)
          {
            mask |= ScanItemMask.Movies;
          }
          movies.Add(movie);
          break;
        case IgnoreItem:
          break;
        default:
          throw new NotImplementedException();
      }
    }

    switch (mask)
    {
      case ScanItemMask.None:
        return new EmptyItem() 
          { 
            Path = path 
          };
      case ScanItemMask.Episodes:
        // We need to construct the 'season' item from 'episodes'.
        //
        foreach (var pattern in this.options.SeasonMatchPatterns)
        {
          var match = Regex.Match(path.Name, pattern);
          if (match.Success)
          {
            var m = new SeasonItemMatch(match);
            return new SeasonItem(episodes) 
              { 
                Title = m.Title,
                Path = path
              };
          }
        }        
        throw new NotImplementedException();
      case ScanItemMask.Seasons:
        // We need to construct the 'shows' item from 'seasons'
        //
        foreach (var pattern in this.options.ShowMatchPatterns)
        {
          var match = Regex.Match(path.Name, pattern);
          if (match.Success)
          {
            var m = new ShowItemMatch(match);
            return new ShowItem(seasons)
              {
                Title = m.Title,
                Path = path
              };
          }
        }
        throw new NotImplementedException();
      case ScanItemMask.Shows:
        // We need to construct the 'library' item from 'shows'
        //
        return new LibraryItem(shows) 
          { 
            Path = path
          };
      case ScanItemMask.Movies:
        // We need to construct the 'library' item from 'movies'
        //
        return new LibraryItem(movies) 
          { 
            Path = path
          };
      case ScanItemMask.Libraries:
        // We need to construct the 'library' item from 'library'
        //
        return new LibraryItem(libraries) 
          { 
            Path = path
          };
      default:
        throw new NotImplementedException($"The scanning of \"{mask}\" isn't supported yet");
    }
  }

  private void ScanLibrary(
    DirectoryPath path)
  {
    var library = this.ScanDirectory(path) switch
    {
      LibraryItem libraryItem => libraryItem,
      _ => throw new NotSupportedException()
    };

    var content = JsonSerializer.Serialize(
      library, 
      new JsonSerializerOptions
      {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
      });

    File.WriteAllText(Path.Combine(path.Value, "this.index.json"), content);
  }

  public override Task<int> ExecuteAsync(
    CommandContext context, 
    ScanCommandSettings settings)
  {
    AnsiConsole
      .Status()
      .Start("Scanning...", ctx => this.ScanLibrary(new DirectoryPath(settings.LibraryPath)));
        
    

    return Task.FromResult(0);
  }
}