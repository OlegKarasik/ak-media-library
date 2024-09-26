using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class EpisodeItemMatch : ItemMatch
{
  public string Title
  {
    get;
  }

  public string Code
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
    this.Code = Required<string>(match, "episode");
  }
}