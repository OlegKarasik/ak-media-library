using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Enrichment.Http.Models;

public record class Episode
{
  [JsonPropertyName("id")]
  public required long Id
  {
    get; init;
  }

  [JsonPropertyName("name")]
  public required string Name
  {
    get; init;
  }

  [JsonPropertyName("isMovie")]
  public required EpisodeKind Kind
  {
    get; init;
  }

  [JsonPropertyName("seasonNumber")]
  public long? SeasonIndex
  {
    get; init;
  }

  [JsonPropertyName("aired")]
  public string? Date
  {
    get; init; 
  }

  [JsonPropertyName("year")]
  public string? Year
  {
    get; init; 
  }

  [JsonPropertyName("overview")]
  public string? Overview
  {
    get; init; 
  }

  [JsonPropertyName("characters")]
  public Character[] Characters
  {
    get; init;
  }

  [JsonPropertyName("nameTranslations")]
  public string[] NameTranslations
  {
    get; init;
  }

  [JsonPropertyName("overviewTranslations")]
  public string[] OverviewTranslations
  {
    get; init;
  }

  public Episode()
  {
    this.Characters = [];
    this.NameTranslations = [];
    this.OverviewTranslations = [];
  }
}
