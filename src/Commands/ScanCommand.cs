using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Extensions;
using MediaLibrary.Extensions.Services;

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

  private Item Scan(
    FilePath path)
  {
    // Rule out all unsupported files
    //
    if (!this.options.VideoExtensions.Contains(path.Extension))
    {
      return NoneItem.Default;
    }
    foreach (var pattern in this.options.IgnoreMatchPatterns)
    {
      if (Regex.IsMatch(path.Name, pattern))
      {
        return NoneItem.Default;
      }
    }

    // Attempt to match the file by one of the episode patterns
    //
    foreach (var pattern in this.options.EpisodeMatchPatterns)
    {
      var match = Regex.Match(path.Name, pattern);
      if (match.Success)
      {
        return new EpisodeItem 
          { 
            Title = match.GetTitle<EpisodeItem>(),
            Position = match.GetPosition<EpisodeItem>(),
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
        return new MovieItem
          {
            Title = match.GetTitle<MovieItem>(),
            Path = path
          };
      }
    }
    throw new NotImplementedException();
  }

  private Item Scan(
    DirectoryPath path)
  {
    var mask = ScanItemMask.None;

    List<SeasonItem> seasons = [];
    List<ShowItem> shows = [];
    List<IndexItem> indices = [];
    foreach (var directoryPath in Directory.EnumerateDirectories(path.Value))
    {
      var directoryItem = this.Scan(new DirectoryPath(directoryPath));
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
        default:
          break;
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
        default:
          break;
      }
    }

    switch (mask)
    {
      case ScanItemMask.None:
        return NoneItem.Default;
      case ScanItemMask.Episodes:
        // We need to construct the 'season' item from 'episodes'.
        //
        foreach (var pattern in this.options.SeasonMatchPatterns)
        {
          var match = Regex.Match(path.Name, pattern);
          if (match.Success)
          {
            var _position = match.GetPosition<SeasonItem>();
            var _episodes = episodes
              .Select(
                episode =>
                {
                  var group = episode.Position.GetGroup();
                  if (episode.Position.HasGroup)
                  {
                    if (group != _position.GetPosition())
                    {
                      throw new NotSupportedException();
                    }
                    return episode;
                  }
                  else
                  {
                    return new EpisodeItem()
                    {
                      Title = episode.Title,
                      Path = episode.Path,
                      Position = ItemPosition.UpdateGroup(episode.Position, _position)
                    };
                  }
                });
            return new SeasonItem
            {
              Title = match.GetTitle<SeasonItem>(),
              Position = match.GetPosition<SeasonItem>(),
              Episodes = _episodes.Collide(i => i.Title),
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
            return new ShowItem
              {
                Title = match.GetTitle<ShowItem>(),
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
            Path = new FilePathIndex(path)
          };
      case ScanItemMask.Movies:
        // We need to construct the 'index' item from 'movies'
        //
        return new IndexItem
          { 
            Shows = [],
            Movies = movies.Collide(i => i.Title),
            Path = new FilePathIndex(path)
          };
      case ScanItemMask.Indices:
        // We need to construct the 'index' item from 'index'
        //
        return new IndexItem
          { 
            Shows = indices.CollideMany(i => i.Shows.Values, i => i.Title),
            Movies = indices.CollideMany(i => i.Movies.Values, i => i.Title),
            Path = new FilePathIndex(path)
          };
      default:
        throw new NotImplementedException($"The scanning of \"{mask}\" isn't supported yet");
    }
  }

  private IndexItem ScanIndex(
    DirectoryPath path)
  {
    return this.Scan(path) switch
    {
      IndexItem item => item,
      _ => throw new NotSupportedException()
    };
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context, 
    ScanCommandSettings settings)
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
              ctx.Status("Scanning...");
              // Scan library directory
              //
              var index = this.ScanIndex(settings.Library);
              
              ctx.Status("Saving...");
              // Save index file
              //
              await FileServices.SaveAsync(index, index.Path);
            });

            AnsiConsole.MarkupLineInterpolated($"Scanning completed [Green]Elapsed: {measurement.Elapsed}[/]");
        });

    return 0;
  }
}