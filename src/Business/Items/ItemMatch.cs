using System.Text.RegularExpressions;
using MediaLibrary.Extensions;

namespace MediaLibrary.Business.Items;
public abstract class ItemMatch
{
  protected static ItemIndex? GetIndex(
    Match match,
    string indexGroup)
  {
    var value = match.Optional<int?>(indexGroup);
    if (value is not null)
    {
      return new ItemIndex([value.Value]);
    }
    return null;
  }

  protected static ItemIndex? GetSpanningIndex(
    Match match,
    string fromGroup,
    string toGroup)
  {
    var from = match.Optional<int?>(fromGroup); 
    var to = match.Optional<int?>(toGroup);
    if (from is not null && to is not null)
    {
      return new ItemIndex([from.Value, to.Value]);
    }
    return null;
  }
}
