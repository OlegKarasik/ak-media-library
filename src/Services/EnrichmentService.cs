using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace MediaLibrary.Extensions.Services;

public class EnrichmentService
{
  public enum SearchTarget
  {
    Movie,
    Series
  }

  public enum EpisodeKind
  {
    Movie = 1,
    Episode = 0
  }

  private class GenericResponse<T>
    where T : class
  {
    [JsonPropertyName("status")]
    public required string Status 
    {
      get; set; 
    }

    [JsonPropertyName("data")]
    public T? Data
    {
      get; set;
    }
  }

  public class SearchData
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

    [JsonPropertyName("overview")]
    public string? Overview
    {
      get; init;
    }
  }

  public class Genre
  {
    [JsonPropertyName("name")]
    public required string Name
    {
      get; init;
    }
  }

  public class Series
  {
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

    [JsonPropertyName("genres")]
    public Genre[]? Genres
    {
      get; init;
    }

    [JsonPropertyName("episodes")]
    public Episode[]? Episodes
    {
      get; init;
    }

    [JsonPropertyName("characters")]
    public Character[]? Characters
    {
      get; init;
    }
  }

  public class Episode
  {
    
    [JsonPropertyName("id")]
    public required long Id
    {
      get; init;
    }

    [JsonPropertyName("name")]
    public required string Name
    {
      get; init;
    }

    [JsonPropertyName("isMovie")]
    public required EpisodeKind Kind
    {
      get; init;
    }

    [JsonPropertyName("aired")]
    public string? Date
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

    [JsonPropertyName("characters")]
    public Character[]? Characters
    {
      get; init;
    }
  }
  
  public class Character
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

  public class AuthorizationTokenSource
  {
    private class LogicData
    {
      [JsonPropertyName("token")]
      public required string Token
      {
        get; init;
      }
    }

    private readonly HttpClient httpClient;
    private readonly IConfigurationRoot configuration;

    public AuthorizationTokenSource(
      HttpClient httpClient,
      IConfigurationRoot configuration)
    {
      this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<string?> GetAccessToken(
      CancellationToken cancellationToken)
    {
      var result = await this.httpClient.PostAsJsonAsync(
        "https://api4.thetvdb.com/v4/login",
        new 
        {
          apikey = this.configuration["enrichment-api-key"]
        },
        cancellationToken);

      result.EnsureSuccessStatusCode();

      var content = await result.Content.ReadFromJsonAsync<GenericResponse<LogicData>>(cancellationToken); 
      return content?.Data?.Token;
    }
  }

  public class Authorization : DelegatingHandler
  {
    private readonly AuthorizationTokenSource source;

    private string? token;

    public Authorization(
      AuthorizationTokenSource source)
    {
      this.source = source ?? throw new ArgumentNullException(nameof(source));

      new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddRetry(new HttpRetryStrategyOptions
          {
            ShouldHandle = args => args.Outcome switch
            {
              { Result.StatusCode: HttpStatusCode.Unauthorized } => PredicateResult.True(),
              _ => PredicateResult.False()
            },
            OnRetry = async (outcome) =>
            {
              this.token = await this.source.GetAccessToken(outcome.Context.CancellationToken);
            }
          });
    }

    protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, 
      CancellationToken cancellationToken)
    {
      request.Headers.Authorization = new AuthenticationHeaderValue(
        "Bearer", 
        this.token ??= await this.source.GetAccessToken(cancellationToken));
      
      return await base.SendAsync(request, cancellationToken);
    }
  }

  private readonly HttpClient httpClient;

  public EnrichmentService(
    HttpClient httpClient)
  {
    this.httpClient = httpClient;
  }

  public async Task<SearchData[]> SearchAsync(
    string title,
    SearchTarget target,
    int offset = 0,
    int limit = 5)
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
    }

    var type = target switch {
      SearchTarget.Series => "series",
      SearchTarget.Movie => "movie",
      _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    var query = HttpUtility.ParseQueryString("");
    query.Add("query", title);
    query.Add("type", type);
    query.Add("offset", offset.ToString());
    query.Add("limit", limit.ToString());

    var result = await this.httpClient.GetFromJsonAsync<GenericResponse<SearchData[]>>(
      $"https://api4.thetvdb.com/v4/search?{query}");

    return result?.Data ?? [];
  }

  public async Task<Series?> GetSeriesAsync(
    long remoteSeriesId)
  {
    var result = await this.httpClient.GetFromJsonAsync<GenericResponse<Series>>(
      $"https://api4.thetvdb.com/v4/series/{remoteSeriesId}/episodes/default");

    return result?.Data;
  }

  public async Task<Episode?> GetEpisodeAsync(
    long remoteEpisodeId)
  {
    var result = await this.httpClient.GetFromJsonAsync<GenericResponse<Episode>>(
      $"https://api4.thetvdb.com/v4/episodes/{remoteEpisodeId}/extended");

    return result?.Data;
  }
}
