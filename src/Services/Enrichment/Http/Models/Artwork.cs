using System.Text.Json.Serialization;

namespace MediaLibrary.Extensions.Services.Enrichment.Http.Models;

public record class Artwork
{
  [JsonPropertyName("id")]
  public required long Id
  {
    get; init;
  }
  
  [JsonPropertyName("image")]
  public required string Image
  {
    get; init;
  }

  [JsonPropertyName("type")]
  public required ArtworkKind Kind
  {
    get; init;
  }

  [JsonPropertyName("score")]
  public required long Score
  {
    get; init;
  }

  [JsonPropertyName("includesText")]
  public required bool IncludesText
  {
    get; init;
  }

  [JsonPropertyName("language")]
  public string? Language
  {
    get; init;
  }
}
