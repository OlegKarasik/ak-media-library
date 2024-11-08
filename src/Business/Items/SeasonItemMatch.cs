using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Items;

public class SeasonItemMatch : ItemMatch
{
  public string Title
  {
    get;
  }

  public ItemIndex SeasonIndex
  {
    get;
  }

  public SeasonItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.SEASON_TITLE);
    
    this.SeasonIndex = 
      GetIndex(match, ItemMatchConstants.SEASON_INDEX) ?? 
      GetSpanningIndex(match, ItemMatchConstants.SEASON_SPAN_INDEX_START, ItemMatchConstants.SEASON_SPAN_INDEX_END) ?? 
      throw new ArgumentException("No season index can be match");;
  }
}
