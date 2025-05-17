namespace MediaLibrary.Business.Enrichment.Models;

public record class Episode
{
  public required Title Title
  {
    get; init;
  }

  public required string Overview
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
