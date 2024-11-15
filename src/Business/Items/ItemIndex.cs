namespace MediaLibrary.Business.Items;

public class ItemIndex : IComparable<ItemIndex>
{
  public static readonly ItemIndex Default = new([1]);

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

  public int CompareTo(
    ItemIndex? other)
  {
    if (other is null)
    {
      return 1;
    }
    return this.Value.CompareTo(other.Value);
  }
}
