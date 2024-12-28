using System.Text.Json.Serialization;

namespace MediaLibrary.Extensions.Services.Enrichment.Http.Models;

public record class Search
{
  [JsonPropertyName("tvdb_id")]
  public required long Id
  {
    get; init;
  }

  [JsonPropertyName("name")]
  public required string Name
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

  [JsonPropertyName("translations")]
  public Dictionary<string, string> NameTranslations
  {
    get; init;
  }

  [JsonPropertyName("overviews")]
  public Dictionary<string, string> OverviewTranslations
  {
    get; init;
  }

  public Search()
  {
    this.NameTranslations = [];
    this.OverviewTranslations = [];
  }
}
