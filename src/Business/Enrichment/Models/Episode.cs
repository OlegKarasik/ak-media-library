namespace MediaLibrary.Business.Enrichment.Models;

public record class Episode
{
  public required string Title
  {
    get; init;
  }

  public required long SeasonIndex
  {
    get; init;
  }

  public string? Date
  {
    get; init;
  }

  public string? Overview
  {
    get; init;
  }

  public Director[] Directors
  {
    get; init;
  }

  public Writer[] Writers
  {
    get; init;
  }

  public Episode()
  {
    this.Directors = [];
    this.Writers = [];
  }
}
