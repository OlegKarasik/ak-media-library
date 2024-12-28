using System.ComponentModel.DataAnnotations;

namespace MediaLibrary.Extensions.Services.Enrichment.Http;

public class EnrichmentHttpCacheOptions
{
  [Required]
  public required string Directory
  {
    get; init;
  }
}
