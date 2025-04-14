namespace MediaLibrary.Business;

public class MediaOverview : MediaString
{
  public MediaOverview(
    string? value)
    
    : base(Init(value))
  {
  }

  private static string Init(
    string? value)
  {
    return value ?? "None";
  }
}
