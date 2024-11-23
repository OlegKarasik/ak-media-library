using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay("<none>")]
public class NoneItem : Item
{
  public static readonly NoneItem Default = new();
}
