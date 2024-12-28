using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MediaLibrary.Extensions.Services.Enrichment.Http;

public class EnrichmentHttpCache
{
  private readonly EnrichmentHttpCacheOptions options;

  private readonly HashAlgorithm hash;
  
  public EnrichmentHttpCache(
    IOptions<EnrichmentHttpCacheOptions> options)
  {
    if (options is null)
    {
      throw new ArgumentNullException(nameof(options));
    }

    this.options = options.Value;
    this.hash = SHA256.Create();
  }
  
  public async Task<string> GetResponseAsync(
    string uri)
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }

    var responseCode = 
      BitConverter.ToString(
        this.hash.ComputeHash(Encoding.Unicode.GetBytes(uri)));

    try
    {
      using var stream = File.Open(this.GetPath(responseCode), FileMode.OpenOrCreate, FileAccess.Read);
      using var r = new StreamReader(stream, leaveOpen: true);
      return await r.ReadToEndAsync();
    }
    catch
    {
      // TODO: Introduce logging.
      return string.Empty;
    }
  }

  public async Task SaveResponseAsync(
    string uri, 
    string response)
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }
    if (string.IsNullOrWhiteSpace(response))
    {
      throw new ArgumentException($"'{nameof(response)}' cannot be null or whitespace.", nameof(response));
    }

    var responseCode = 
      BitConverter.ToString(
        this.hash.ComputeHash(Encoding.Unicode.GetBytes(uri)));

    try
    {
      using var stream = File.Open(this.GetPath(responseCode), FileMode.OpenOrCreate, FileAccess.Write);
      using var w = new StreamWriter(stream, leaveOpen: true);
      await w.WriteAsync(response);
    }
    catch
    {
      // TODO: Introduce logging
    }
  }

  private string GetPath(
    string responseCode)
  {
    if (string.IsNullOrWhiteSpace(responseCode))
    {
      throw new ArgumentException($"'{nameof(responseCode)}' cannot be null or whitespace.", nameof(responseCode));
    }
    return Path.Combine(this.options.Directory, responseCode);
  }
}
