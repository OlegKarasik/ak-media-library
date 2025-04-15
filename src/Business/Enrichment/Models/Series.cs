namespace MediaLibrary.Business.Enrichment.Models;

public record class Series
{
  public required string Title
  {
    get; init;
  }

  public required string Overview
  {
    get; init;
  }

  public string? Date
  {
    get; init;
  }

  public string? Year
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

  public string[] Genres
  {
    get; init;
  }

  public Dictionary<long, Season> Seasons
  {
    get; init;
  }

  public Series()
  {
    this.Genres = [];
    this.Seasons = [];
  }
}
