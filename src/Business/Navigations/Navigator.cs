using MediaLibrary.Business.Items;

namespace MediaLibrary.Business.Navigation;

public enum NavigationRoot
{
  Movies,
  Shows
}

public class NavigationPath
{
  public NavigationRoot Root
  {
    get;
  }
}

public class Navigator
{
  public static Item GetItem(
    LibraryItem library,
    NavigationPath path)
  {
    if (library is null)
    {
      throw new ArgumentNullException(nameof(library));
    }
    if (path is null)
    {
      throw new ArgumentNullException(nameof(path));
    }

    NavigationSegment segment = path.Root switch
    {
      NavigationRoot.Movies => new MoviesNavigationSegment(library),
      NavigationRoot.Shows => new ShowsNavigationSegment(library),
      _ => throw new NotImplementedException()
    };


    return null;
  }
}
