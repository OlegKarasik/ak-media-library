using System.Text.Json.Serialization;

namespace MediaLibrary.Extensions.Services.Enrichment.Http.Models;

public record class Genre
{
  [JsonPropertyName("name")]
  public required string Name
  {
    get; init;
  }
}
