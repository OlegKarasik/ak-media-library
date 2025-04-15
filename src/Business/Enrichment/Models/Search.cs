namespace MediaLibrary.Business.Enrichment.Models;

public record class Search
{
  public required long Id
  {
    get; init;
  }

  public required string Title
  {
    get; init;
  }

  public required string Overview
  {
    get; init;
  }

  public string? Year
  {
    get; init;
  }
}
