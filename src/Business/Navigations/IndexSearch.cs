using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearch
{
  public static Item GetItem(
    IndexItem index,
    IndexQuery query)
  {
    if (index is null)
    {
      throw new ArgumentNullException(nameof(index));
    }
    if (query is null)
    {
      throw new ArgumentNullException(nameof(query));
    }

    IndexSearchPosition segment = query.Root switch
    {
      IndexQueryRoot.Movies => new IndexSearchPositionAtMovieCollection(index),
      IndexQueryRoot.Shows => new IndexSearchPositionAtShowCollection(index),
      _ => throw new NotImplementedException()
    };
    
    foreach (var section in query.Sections)
    {
      segment = segment[section];
    }

    return segment.Current;
  }
}
