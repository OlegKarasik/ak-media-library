using System.Text.RegularExpressions;
using MediaLibrary.Business.Items;
using MediaLibrary.Extensions;

namespace MediaLibrary.Commands.Matching;

public class DecodeSeasonItemMatch : DecodeItemMatch
{
  public string Title
  {
    get;
  }

  public ItemPosition SeasonIndex
  {
    get;
  }

  public DecodeSeasonItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.SEASON_TITLE).Trim();
    
    this.SeasonIndex = 
      GetIndex(match, ItemMatchConstants.SEASON_POSITION) ?? 
      GetSpanningIndex(match, ItemMatchConstants.SEASON_SPAN_POSITION_START, ItemMatchConstants.SEASON_SPAN_POSITION_END) ?? 
      throw new ArgumentException("No season index can be match");;
  }
}
