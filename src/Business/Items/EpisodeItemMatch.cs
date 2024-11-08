using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Items;

public class EpisodeItemMatch : ItemMatch
{
  public string Title
  {
    get;
  }

  public ItemIndex SeasonIndex
  {
    get;
  }

  public ItemIndex EpisodeIndex
  {
    get;
  }

  public EpisodeItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.EPISODE_TITLE);

    this.SeasonIndex  = GetIndex(match, ItemMatchConstants.SEASON_INDEX) ?? ItemIndex.Default;
    this.EpisodeIndex = 
      GetIndex(match, ItemMatchConstants.EPISODE_INDEX) ?? 
      GetSpanningIndex(match, ItemMatchConstants.EPISODE_SPAN_INDEX_START, ItemMatchConstants.EPISODE_SPAN_INDEX_END) ?? 
      throw new ArgumentException("No episode index can be match");;
  }
}