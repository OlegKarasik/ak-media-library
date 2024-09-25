using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class SeasonItemMatch : ItemMatch
{
  public string Title
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

    this.Title = Required<string>(match, "title");
  }
}
