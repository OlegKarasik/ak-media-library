using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Commands.Matching;

public class DecodeShowItemMatch : DecodeItemMatch
{
  public string Title
  {
    get;
  }

  public DecodeShowItemMatch(
    Match match)
  {
    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    this.Title = match.Required<string>(ItemMatchConstants.SHOW_TITLE).Trim();
  }
}
