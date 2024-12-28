namespace MediaLibrary.Extensions.Services.Enrichment.Models;

public record class Season
{
  public required long Index
  {
    get; init;
  }

  public string? Overview
  {
    get; init;
  }

  public byte[]? Image
  {
    get; init;
  }

  public byte[]? ImageBackground
  {
    get; init;
  }

  public Dictionary<string, Episode> Episodes
  {
    get; init;
  }

  public Season()
  {
    this.Episodes = [];
  }
}
