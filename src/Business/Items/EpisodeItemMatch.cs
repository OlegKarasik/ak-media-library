using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Items;

public class EpisodeItemMatch : ItemMatch
{
  public string Title
  {
    get;
  }

  public EpisodeItemIndex Index
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

    this.Title = Required<string>(match, "title");
    this.Index = GetIndex(match) ?? GetSpanningIndex(match) ?? throw new ArgumentException("No episode index can be match");
  }

  private static EpisodeItemIndex? GetIndex(
    Match match)
  {
    var value = Optional<int?>(match, "index");
    if (value is not null)
    {
      return new EpisodeItemIndex([value.Value]);
    }
    return null;
  }

  private static EpisodeItemIndex? GetSpanningIndex(
    Match match)
  {
    var from = Optional<int?>(match, "from"); 
    var to = Optional<int?>(match, "to");
    if (from is not null && to is not null)
    {
      return new EpisodeItemIndex([from.Value, to.Value]);
    }
    return null;
  }
}