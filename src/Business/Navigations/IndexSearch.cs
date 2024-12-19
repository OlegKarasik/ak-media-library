using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class IndexSearch
{
  public static Item GetItem(
    IndexItem index,
    IndexSearchRequest request)
  {
    if (index is null)
    {
      throw new ArgumentNullException(nameof(index));
    }
    if (request is null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    IndexSearchPosition position = request.Root switch
    {
      IndexSearchRoot.Movies => new IndexSearchPositionAtMovieCollection(index),
      IndexSearchRoot.Shows => new IndexSearchPositionAtShowCollection(index),
      _ => throw new NotImplementedException()
    };
    
    foreach (var section in request.Sections)
    {
      position = position[section];
    }

    return position.Current;
  }
}
