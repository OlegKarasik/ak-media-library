using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class EpisodeItemMatch : ItemMatch
{
  public string Title
  {
    get;
  }

  public long Position
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
    this.Position = Required<long>(match, "episode");
  }
}