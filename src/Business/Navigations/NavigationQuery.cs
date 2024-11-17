namespace MediaLibrary.Business.Navigation;

public class NavigationQuery
{
  public NavigationQueryRoot Root
  {
    get;
  }

  public IEnumerable<string> Sections
  {
    get;
  }
}
