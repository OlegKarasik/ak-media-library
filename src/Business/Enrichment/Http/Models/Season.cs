using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Enrichment.Http.Models;

public record class Season
{
  [JsonPropertyName("id")]
  public required long Id
  {
    get; init;
  }

  [JsonPropertyName("number")]
  public required long Index
  {
    get; init;
  }

  [JsonPropertyName("year")]
  public string? Year
  {
    get; init; 
  }

  [JsonPropertyName("image")]
  public string? Image
  {
    get; init;
  }

  public string? Overview
  {
    get; init;
  }

  [JsonPropertyName("artwork")]
  public Artwork[] Artworks
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

  public Season()
  {
    this.Artworks = [];
    this.NameTranslations = [];
    this.OverviewTranslations = [];
  }
}
