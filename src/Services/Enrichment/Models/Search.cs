namespace MediaLibrary.Extensions.Services.Enrichment.Models;

public record class Search
{
  public required long Id
  {
    get; init;
  }

  public required string Name
  {
    get; init;
  }

  public string? Year
  {
    get; init;
  }

  public string? Overview
  {
    get; init;
  }
}
