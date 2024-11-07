using System.Text.RegularExpressions;

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

    this.Title = Required<string>(match, "title");
  }
}
