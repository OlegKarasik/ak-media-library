using System.Text.RegularExpressions;
using MediaLibrary.Business.Items;
using MediaLibrary.Extensions;

namespace MediaLibrary.Commands.Matching;

public class DecodeEpisodeItemMatch : DecodeItemMatch
{
  public string Title
  {
    get;
  }

  public ItemPosition SeasonIndex
  {
    get;
  }

  public ItemPosition EpisodeIndex
  {
    get;
  }

  public DecodeEpisodeItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.EPISODE_TITLE).Trim();

    this.SeasonIndex  = GetIndex(match, ItemMatchConstants.SEASON_POSITION) ?? ItemPosition.Default;
    this.EpisodeIndex = 
      GetIndex(match, ItemMatchConstants.EPISODE_POSITION) ?? 
      GetSpanningIndex(match, ItemMatchConstants.EPISODE_SPAN_POSITION_START, ItemMatchConstants.EPISODE_SPAN_POSITION_END) ?? 
      throw new ArgumentException("No episode index can be match");;
  }
}