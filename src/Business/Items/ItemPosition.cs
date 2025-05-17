using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Items;

public class ItemPosition : IComparable<ItemPosition>
{
  private const ulong GROUP_BIT = 1UL << 63;
  private const ulong SPAN_BIT  = 1UL << 62;

  public static readonly ItemPosition Default = new(0);

  [JsonIgnore]
  public bool HasGroup => (this.Value & GROUP_BIT) != 0;
  
  [JsonIgnore]
  public bool HasSpan => (this.Value & SPAN_BIT) != 0;

  public ulong Value 
  {
    get;
  }

  [JsonConstructor]
  public ItemPosition(
    ulong value)
  {
    this.Value = value;
  }

  public ItemPosition(
    ushort value)
  {
    this.Value = SetPosition(value);
  }

  public ItemPosition(
    ushort open, ushort close)
  {
    this.Value = SetSpanPosition(open, close);
  }

  public ItemPosition(
    byte group, ushort value)
  {
    this.Value = SetGroup(group) | SetPosition(value);
  }

  public ItemPosition(
    byte group, ushort open, ushort close)
  {
    this.Value = SetGroup(group) | SetSpanPosition(open, close);
  }

  public ItemPosition(
    ItemPosition group, ItemPosition position)
  {
    if (position.HasSpan)
    {
      var (Open, Close) = position.GetSpanPosition();
      this.Value = SetGroup(group.GetPosition()) | SetSpanPosition(Open, Close);
    }
    else
    {
      this.Value = SetGroup(group.GetPosition()) | SetPosition(position.GetPosition());
    }
  }

  private static ulong SetPosition(
    ulong value)
  {
    return value & 0xFFFF;
  }

  private static ulong SetSpanPosition(
    ulong open, ulong close)
  {
    return (open << 32) | (close << 16) | (((close - open) / 2) + open) | SPAN_BIT;
  }

  private static ulong SetGroup(
    ulong value)
  {
    return (value << 48) | GROUP_BIT;
  }

  public ulong GetPosition()
  {
    return this.Value & 0xFFFF;
  }

  public ulong GetGroup()
  {
    return (this.Value & 0x3FF000000000000) >> 48;
  }

  public (ulong Open, ulong Close) GetSpanPosition()
  {
    return ((this.Value & 0xFFFF00000000) >> 32, (this.Value & 0xFFFF0000) >> 16);
  }

  public int CompareTo(
    ItemPosition? other)
  {
    if (other is null)
    {
      return 1;
    }
    var result = this.GetGroup().CompareTo(other.GetGroup());
    if (result == 0)
    {
      result = this.GetPosition().CompareTo(other.GetPosition());
    }
    return result;
  }

  public override string ToString()
  {
    if (this.HasGroup)
    {
      if (this.HasSpan)
      {
        var (Open, Close) = this.GetSpanPosition();
        return $"G: {this.GetGroup()}, O/C: {Open}/{Close}, V: {this.GetPosition()}";
      }
      return $"G: {this.GetGroup()}, V: {this.GetPosition()}";
    }
    if (this.HasSpan)
    {
      var (Open, Close) = this.GetSpanPosition();
      return $"O/C: {Open}/{Close}, V: {this.GetPosition()}";
    }
    return $"V: {this.GetPosition()}";
  }
}
