using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Items;

public class ShowItemMatch : ItemMatch
{
  public string Title
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

    this.Title = match.Required<string>(ItemMatchConstants.SHOW_TITLE).Trim();
  }
}
