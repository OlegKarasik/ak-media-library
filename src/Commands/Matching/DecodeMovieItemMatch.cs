using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Commands.Matching;

public class DecodeMovieItemMatch : DecodeItemMatch
{
  public string Title
  {
    get;
  }

  public DecodeMovieItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.MOVIE_TITLE).Trim();
  }
}