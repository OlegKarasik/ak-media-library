using System.ComponentModel.DataAnnotations;

namespace MediaLibrary.Business.Enrichment.Http;

public class EnrichmentHttpCacheOptions
{
  [Required]
  public required string Directory
  {
    get; init;
  }
}
