namespace MediaLibrary.Business.Items;

public class ItemIndex
{
  public static readonly ItemIndex Default = new([0]);

  public int[] Values
  {
    get;
  }

  public int Value
  {
    get;
  }

  public ItemIndex(
    int[] values)
  {
    this.Values = values ?? throw new ArgumentNullException(nameof(values));
    this.Value = (int)values.Average();
  }
}
