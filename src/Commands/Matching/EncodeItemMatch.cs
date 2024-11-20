using System.Text.RegularExpressions;
using MediaLibrary.Commands.Matching;
using MediaLibrary.Extensions;

namespace MediaLibrary.Commands;

public abstract partial class EncodeItemMatch
{
  protected virtual string EpisodePosition => "";

  protected virtual string EpisodeSpanPositionStart => "";

  protected virtual string EpisodeSpanPositionEnd => "";

  protected virtual string EpisodeTitle => "";

  protected virtual string SeasonPosition => "";

  protected virtual string SeasonSpanPositionStart => "";

  protected virtual string SeasonSpanPositionEnd => "";

  protected virtual string SeasonTitle => "";

  protected virtual string ShowTitle => "";

  protected virtual string MovieTitle => "";

  public string Encode(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    var value = match.Optional("Match");
    return value switch
      {
        ItemMatchConstants.EPISODE_POSITION => this.EpisodePosition,
        ItemMatchConstants.EPISODE_SPAN_POSITION_START => this.EpisodeSpanPositionStart,
        ItemMatchConstants.EPISODE_SPAN_POSITION_END => this.EpisodeSpanPositionEnd,
        ItemMatchConstants.EPISODE_TITLE => this.EpisodeTitle,
        ItemMatchConstants.SEASON_POSITION => this.SeasonPosition,
        ItemMatchConstants.SEASON_SPAN_POSITION_START => this.SeasonSpanPositionStart,
        ItemMatchConstants.SEASON_SPAN_POSITION_END => this.SeasonSpanPositionEnd,
        ItemMatchConstants.SEASON_TITLE => this.SeasonTitle,
        ItemMatchConstants.SHOW_TITLE => this.ShowTitle,
        ItemMatchConstants.MOVIE_TITLE => this.MovieTitle,
        _ => "",
      };
  }
}
