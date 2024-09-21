using System.Diagnostics;

namespace MediaLibrary.Business.Items;

[DebuggerDisplay($"{{{nameof(Title)}}}")]
public class MovieItem : FileItem
{
  public required string Title
  {
    get; init;
  }
}
