namespace MediaLibrary.Business.Enrichment.Models;

public record class Search
{
  public required long Id
  {
    get; init;
  }

  public required MediaTitle Title
  {
    get; init;
  }

  public required MediaOverview Overview
  {
    get; init;
  }

  public string? Year
  {
    get; init;
  }
}
