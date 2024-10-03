namespace MediaLibrary.Business.Items;

public class IndexItem
{
  public Dictionary<string, MovieItem> Movies 
  { 
    get; init; 
  }

  public Dictionary<string, Dictionary<string, Dictionary<string, EpisodeItem>>> Shows 
  { 
    get; init; 
  }

  public IndexItem(
    LibraryItem library)
  {
    foreach (var movie in library.Movies)
    {

    }
    foreach (var show in library.Shows)
    {

    }
  }
}
