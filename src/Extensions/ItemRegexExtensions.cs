using System.Text.RegularExpressions;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Extensions;

public static class ItemRegexExtensions
{
  private const string MOVIE_TITLE = "MovieTitle";
  private const string SHOW_TITLE = "ShowTitle";
  private const string SEASON_TITLE = "SeasonTitle";
  private const string SEASON_POSITION = "Season";
  private const string SEASON_SPAN_POSITION_START = "SeasonOpen";
  private const string SEASON_SPAN_POSITION_END = "SeasonClose";
  private const string EPISODE_TITLE = "EpisodeTitle";
  private const string EPISODE_POSITION = "Episode";
  private const string EPISODE_SPAN_POSITION_START = "EpisodeOpen";
  private const string EPISODE_SPAN_POSITION_END = "EpisodeClose";

  private static readonly Dictionary<Type, string> title;
  private static readonly Dictionary<Type, (string group, string value, string open, string close)> position;

  static ItemRegexExtensions()
  {
    title = new Dictionary<Type, string> {
      [typeof(MovieItem)]   = MOVIE_TITLE,
      [typeof(EpisodeItem)] = EPISODE_TITLE,
      [typeof(SeasonItem)]  = SEASON_TITLE,
      [typeof(ShowItem)]    = SHOW_TITLE
    };
    position = new Dictionary<Type, (string group, string value, string open, string close)> {
      [typeof(EpisodeItem)] = (SEASON_POSITION, EPISODE_POSITION, EPISODE_SPAN_POSITION_START, EPISODE_SPAN_POSITION_END),
      [typeof(SeasonItem)]  = (string.Empty,    SEASON_POSITION,  SEASON_SPAN_POSITION_START,  SEASON_SPAN_POSITION_END)
    };
  }

  public static string GetTitle<T>(
    this Match match)

    where T: Item
  {
    return title.TryGetValue(typeof(T), out var group) 
      ? match.Optional<string>(group) ?? string.Empty
      : string.Empty;
  }

  public static ItemPosition GetPosition<T>(
    this Match match)

    where T: Item
  {
    if (position.TryGetValue(typeof(T), out var groups))
    {
      var group = match.Optional<byte?>  (groups.group);
      var value = match.Optional<ushort?>(groups.value);
      var open  = match.Optional<ushort?>(groups.open); 
      var close = match.Optional<ushort?>(groups.close);
      if (group is not null)
      {
        if (value is not null)
        {
          return new ItemPosition(group.Value, value.Value);
        }
        if (open is not null && close is not null)
        {
          return new ItemPosition(group.Value, open.Value, close.Value);
        }
      }
      else
      {
        if (value is not null)
        {
          return new ItemPosition(value.Value);
        }
        if (open is not null && close is not null)
        {
          return new ItemPosition(open.Value, close.Value);
        }
      }
      return ItemPosition.Default;
    }
    return ItemPosition.Default;
  }
}
