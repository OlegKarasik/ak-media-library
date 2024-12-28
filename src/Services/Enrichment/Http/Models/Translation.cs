using System.Text.Json.Serialization;

namespace MediaLibrary.Extensions.Services.Enrichment.Http.Models;

public record class Translation
{
  [JsonPropertyName("name")]
  public string? Name
  {
    get; init;
  }

  [JsonPropertyName("overview")]
  public string? Overview
  {
    get; init;
  }
}
