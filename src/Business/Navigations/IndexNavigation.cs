using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearch
{
  public static Item GetItem(
    IndexItem library,
    IndexQuery query)
  {
    if (library is null)
    {
      throw new ArgumentNullException(nameof(library));
    }
    if (query is null)
    {
      throw new ArgumentNullException(nameof(query));
    }

    IndexSegment segment = query.Root switch
    {
      IndexQueryRoot.Movies => new MoviesIndexSegment(library),
      IndexQueryRoot.Shows => new ShowsIndexSegment(library),
      _ => throw new NotImplementedException()
    };
    
    foreach (var section in query.Sections)
    {
      segment = segment[section];
    }

    return segment.Current;
  }
}
