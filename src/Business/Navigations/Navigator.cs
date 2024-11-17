using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public class Navigator
{
  public static Item GetItem(
    LibraryItem library,
    NavigationQuery query)
  {
    if (library is null)
    {
      throw new ArgumentNullException(nameof(library));
    }
    if (query is null)
    {
      throw new ArgumentNullException(nameof(query));
    }

    NavigationSegment segment = query.Root switch
    {
      NavigationQueryRoot.Movies => new MoviesNavigationSegment(library),
      NavigationQueryRoot.Shows => new ShowsNavigationSegment(library),
      _ => throw new NotImplementedException()
    };
    
    foreach (var section in query.Sections)
    {
      segment = segment[section];
    }

    return segment.Current;
  }
}
