using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MediaLibrary.Business.Items;


[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class EpisodeItem : FileItem
{
  public required string Title
  { 
    get; init;
  }
}
