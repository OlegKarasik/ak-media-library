namespace MediaLibrary.Business.Enrichment.Models;

public record class Season
{
  public required long Index
  {
    get; init;
  }

  public required string Overview
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

  public Dictionary<Title, Episode> Episodes
  {
    get; init;
  }

  public Season()
  {
    this.Episodes = [];
  }
}
