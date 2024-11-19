namespace MediaLibrary.Business.Items;

public class ItemPosition : IComparable<ItemPosition>
{
  public static readonly ItemPosition Default = new([1]);

  public int[] Values
  {
    get;
  }

  public int Value
  {
    get;
  }
  
  public bool IsSpanning => this.Values.Length != 1;

  public int ValueStart => this.Values.First();

  public int ValueEnd => this.Values.Last();


  public ItemPosition(
    int[] values)
  {
    this.Values = values ?? throw new ArgumentNullException(nameof(values));
    this.Value = (int)values.Average();
  }

  public int CompareTo(
    ItemPosition? other)
  {
    if (other is null)
    {
      return 1;
    }
    return this.Value.CompareTo(other.Value);
  }
}
