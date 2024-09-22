using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class EpisodeItemMatch : ItemMatch
{
  public string? Title
  {
    get;
  }

  public long? SeasonPosition
  {
    get;
  }

  public long? EpisodePosition
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

    this.Title = this.Get<string>(match, "title");
    this.SeasonPosition = this.Get<long>(match, "season");
    this.EpisodePosition = this.Get<long>(match, "episode");
  }
}