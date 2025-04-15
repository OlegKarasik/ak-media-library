namespace MediaLibrary.Business.Enrichment.Models;

public record class Season
{
  public required long Index
  {
    get; init;
  }

  public required MediaOverview Overview
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
