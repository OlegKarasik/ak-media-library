using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Items;

public class MovieItemMatch : ItemMatch
{
  public string Title
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

    this.Title = match.Required<string>(ItemMatchConstants.MOVIE_TITLE).Trim();
  }
}