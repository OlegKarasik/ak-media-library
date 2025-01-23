using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Enrichment.Http.Models;

public record class Genre
{
  [JsonPropertyName("name")]
  public required string Name
  {
    get; init;
  }
}
