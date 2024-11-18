using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using MediaLibrary.Extensions;

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
    Indices   = 0b00010000
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
    if (!this.options.VideoExtensions.Contains(path.Extension))
    {
      return new IgnoreItem
      {
        Path = path
      };
    }
    foreach (var pattern in this.options.IgnoreMatchPatterns)
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
            SeasonPosition = m.SeasonIndex,
            EpisodePosition = m.EpisodeIndex,
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
    List<IndexItem> indices = [];
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
        case IndexItem index:
          if (indices.Count == 0)
          {
            mask |= ScanItemMask.Indices;
          }
          indices.Add(index);
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
        return new EmptyItem
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
            return new SeasonItem
              { 
                Title = m.Title,
                SeasonPosition = m.SeasonIndex,
                Episodes = episodes.Collide(i => i.Title),
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
            return new ShowItem
              {
                Title = m.Title,
                Seasons = seasons.Collide(i => i.Title),
                Path = path
              };
          }
        }
        throw new NotImplementedException();
      case ScanItemMask.Shows:
        // We need to construct the 'index' item from 'shows'
        //
        return new IndexItem
          { 
            Shows = shows.Collide(i => i.Title),
            Movies = [],
            Path = path
          };
      case ScanItemMask.Movies:
        // We need to construct the 'index' item from 'movies'
        //
        return new IndexItem
          { 
            Shows = [],
            Movies = movies.Collide(i => i.Title),
            Path = path
          };
      case ScanItemMask.Indices:
        // We need to construct the 'index' item from 'index'
        //
        return new IndexItem
          { 
            Shows = indices.SelectMany(i => i.Shows.Values).Collide(i => i.Title),
            Movies = indices.SelectMany(i => i.Movies.Values).Collide(i => i.Title),
            Path = path
          };
      default:
        throw new NotImplementedException($"The scanning of \"{mask}\" isn't supported yet");
    }
  }

  private void ScanLibrary(
    DirectoryPath path)
  {
    var index = this.ScanDirectory(path) switch
    {
      IndexItem item => item,
      _ => throw new NotSupportedException()
    };

    var content = JsonSerializer.Serialize(
      index, 
      new JsonSerializerOptions
      {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
      });

    File.WriteAllText(new IndexPath(path.Value).Value, content);
  }

  public override Task<int> ExecuteAsync(
    CommandContext context, 
    ScanCommandSettings settings)
  {
    AnsiConsole
      .Status()
      .Start("Scanning...", ctx => this.ScanLibrary(settings.Directory));
        
    

    return Task.FromResult(0);
  }
}