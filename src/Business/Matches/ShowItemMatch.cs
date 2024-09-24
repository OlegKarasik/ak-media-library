using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Matches;

public class ShowItemMatch : ItemMatch
{
  public string? Title
  {
    get;
  }

  public ShowItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = this.Get<string>(match, "title");
  }
}
