using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class MovieItemMatch : ItemMatch
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

  public MovieItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = this.Get<string>(match, "title");
  }
}