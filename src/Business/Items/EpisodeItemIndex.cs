namespace MediaLibrary.Business.Items;

public class EpisodeItemIndex
{
  public int[] Values
  {
    get;
  }

  public int Value
  {
    get;
  }

  public EpisodeItemIndex(
    int[] values)
  {
    this.Values = values ?? throw new ArgumentNullException(nameof(values));
    this.Value = (int)values.Average();
  }
}
