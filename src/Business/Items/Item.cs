namespace MediaLibrary.Business.Items;

public class Item
{
}

public class Item<T> : Item
{
  public required T Path
  {
    get; init;
  }
}
