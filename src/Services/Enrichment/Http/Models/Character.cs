using System.Text.Json.Serialization;

namespace MediaLibrary.Extensions.Services.Enrichment.Http.Models;

public record class Character
{
  [JsonPropertyName("name")]
  public string? Name
  {
    get; init;
  }

  [JsonPropertyName("personName")]
  public required string PersonName 
  { 
    get; init; 
  }

  [JsonPropertyName("peopleType")]
  public required string PersonType 
  {
    get; init;
  }
}
