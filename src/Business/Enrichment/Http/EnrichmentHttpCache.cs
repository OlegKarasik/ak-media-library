using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MediaLibrary.Business.Enrichment.Http;

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
  
  public async Task<string> GetResponseStringAsync(
    string uri)
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }

    return await this.GetResponseInternalAsync(uri, Read, string.Empty);

    static async Task<string> Read(Stream stream)
    {
      using var r = new StreamReader(stream, leaveOpen: true);
      return await r.ReadToEndAsync();
    }
  }

  public async Task<byte[]> GetResponseBytesAsync(
    string uri)
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }

    return await this.GetResponseInternalAsync(uri, Read, []);

    static async Task<byte[]> Read(Stream stream)
    {
      var bytes = new byte[stream.Length];

      await stream.ReadExactlyAsync(bytes);

      return bytes;
    }
  }

  public async Task SaveResponseAsync(
    string uri, 
    string response)
  {
    await this.SaveResponseInternalAsync(uri, response, Write);

    static async Task Write(Stream stream, string response)
    {
      using var w = new StreamWriter(stream, leaveOpen: true);
      await w.WriteAsync(response);
    }
  }

  public async Task SaveResponseAsync(
    string uri,
    byte[] response)
  {
    await this.SaveResponseInternalAsync(uri, response, Write);

    static async Task Write(Stream stream, byte[] response)
    {
      await stream.WriteAsync(response);
    }
  }

  private async Task<T> GetResponseInternalAsync<T>(
    string uri,
    Func<Stream, Task<T>> readFn,
    T defaultResponse)

    where T: class
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }
    if (readFn is null)
    {
      throw new ArgumentNullException(nameof(readFn));
    }

    var responseCode = this.GetResponseCode(uri);

    try
    {
      using var stream = File.Open(this.GetPath(responseCode), FileMode.OpenOrCreate, FileAccess.Read);
      return await readFn(stream);
    }
    catch
    {
      // TODO: Introduce logging.
      return defaultResponse;
    }
  }

  private async Task SaveResponseInternalAsync<T>(
    string uri,
    T response,
    Func<Stream, T, Task> writeFn)

    where T: class
  {
    if (string.IsNullOrWhiteSpace(uri))
    {
      throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
    }
    if (response is null)
    {
      throw new ArgumentNullException(nameof(response));
    }
    if (writeFn is null)
    {
      throw new ArgumentNullException(nameof(writeFn));
    }

    var responseCode = this.GetResponseCode(uri);

    try
    {
      using var stream = File.Open(this.GetPath(responseCode), FileMode.OpenOrCreate, FileAccess.Write);
      await writeFn(stream, response);
    }
    catch
    {
      // TODO: Introduce logging
    }
  }

  private string GetResponseCode(
    string uri)
  {
    return BitConverter.ToString(
      this.hash.ComputeHash(Encoding.Unicode.GetBytes(uri)));
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
