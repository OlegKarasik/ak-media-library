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
  private const string None = "";

  public enum ArtworkKind
  {
    SeriesBanner = 1,
    SeriesPoster = 2,
    SeriesBackground = 3,
    SeriesIcon = 5,
    SeasonBanner = 6,
    SeasonPoster = 7,
    SeasonBackground = 8,
    SeasonIcon = 10
  }

  public enum EpisodeKind
  {
    Movie = 1,
    Episode = 0
  }

  public enum SearchTarget
  {
    Movie,
    Series
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
    public required T Data
    {
      get; set;
    }
  }

  public record class SearchResult
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

    [JsonPropertyName("year")]
    public string Year
    {
      get; init;
    }

    [JsonPropertyName("overview")]
    public string Overview
    {
      get; init;
    }

    [JsonPropertyName("overviews")]
    public Dictionary<string, string> Overviews
    {
      get; init;
    }

    public SearchResult()
    {
      this.Year = None;
      this.Overview = None;
      this.Overviews = [];
    }
  }

  public record class Translation
  {
    [JsonPropertyName("overview")]
    public required string Overview
    {
      get; init;
    }
  }

  public record class Genre
  {
    [JsonPropertyName("name")]
    public required string Name
    {
      get; init;
    }
  }

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
    public string Language
    {
      get; init;
    }

    public Artwork()
    {
      this.Language = None;
    }
  }

  public record class Series
  {
    public record class Season
    {
      [JsonPropertyName("id")]
      public required long Id
      {
        get; init;
      }

      [JsonPropertyName("number")]
      public required long Index
      {
        get; init;
      }

      [JsonPropertyName("overviewTranslations")]
      public string[] SupportedTranslations
      {
        get; init;
      }

      public Season()
      {
        this.SupportedTranslations = [];
      }
    }

    [JsonPropertyName("name")]
    public required string Name
    {
      get; init;
    }

    [JsonPropertyName("firstAired")]
    public required string Date
    {
      get; init;
    }

    [JsonPropertyName("year")]
    public string Year
    {
      get; init;
    }

    [JsonPropertyName("overview")]
    public string Overview
    {
      get; init;
    }

    [JsonPropertyName("genres")]
    public Genre[] Genres
    {
      get; init;
    }

    [JsonPropertyName("seasons")]
    public Season[] Seasons
    {
      get; init;
    }

    [JsonPropertyName("characters")]
    public Character[] Characters
    {
      get; init;
    }

    [JsonPropertyName("artworks")]
    public Artwork[] Artworks
    {
      get; init;
    }

    [JsonPropertyName("overviewTranslations")]
    public string[] SupportedTranslations
    {
      get; init;
    }

    public Series()
    {
      this.Year = None;
      this.Overview = None;
      this.Genres = [];
      this.Seasons = [];
      this.Characters = [];
      this.Artworks = [];
      this.SupportedTranslations = [];
    }
  }

  public record class Season
  {
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

      [JsonPropertyName("overview")]
      public string Overview
      {
        get; init; 
      }

      public Episode()
      {
        this.Overview = None;
      }
    }

    [JsonPropertyName("id")]
    public required long Id
    {
      get; init;
    }

    [JsonPropertyName("number")]
    public required long Index
    {
      get; init;
    }

    [JsonPropertyName("year")]
    public string Year
    {
      get; init; 
    }

    [JsonPropertyName("image")]
    public string Image
    {
      get; init;
    }

    public string Overview
    {
      get; init;
    }

    [JsonPropertyName("episodes")]
    public Episode[] Episodes
    {
      get; init;
    }

    [JsonPropertyName("artwork")]
    public Artwork[] Artworks
    {
      get; init;
    }

    [JsonPropertyName("overviewTranslations")]
    public string[] SupportedTranslations
    {
      get; init;
    }

    public Season()
    {
      this.Overview = None;
      this.Year = None;
      this.Image = None;
      this.Episodes = [];
      this.Artworks = [];
      this.SupportedTranslations = [];
    }
  }

  public record class Episode
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
    public string Date
    {
      get; init; 
    }

    [JsonPropertyName("year")]
    public string Year
    {
      get; init; 
    }

    [JsonPropertyName("overview")]
    public string Overview
    {
      get; init; 
    }

    [JsonPropertyName("characters")]
    public Character[] Characters
    {
      get; init;
    }

    [JsonPropertyName("overviewTranslations")]
    public string[] SupportedTranslations
    {
      get; init;
    }

    public Episode()
    {
      this.Date = None;
      this.Year = None;
      this.Overview = None;
      this.Characters = [];
      this.SupportedTranslations = [];
    }
  }
  
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

  private async Task<T> GetAsync<T>(
    string uri)

    where T : class
  {
    var result = await this.httpClient.GetFromJsonAsync<GenericResponse<T>>(uri);
    return result!.Data;
  }

  public async Task<SearchResult[]?> SearchAsync(
    string title,
    SearchTarget target,
    string language = "eng",
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
    query.Add("language", language);
    query.Add("offset", offset.ToString());
    query.Add("limit", limit.ToString());

    var results = await this.GetAsync<SearchResult[]>(
      $"https://api4.thetvdb.com/v4/search?{query}");

    return [.. results.Select(
      result => result with {
        Overview = result.Overviews.TryGetValue(language, out var overview) ? overview : result.Overview
      })];
  }

  public async Task<Series> GetSeriesAsync(
    long id,
    string language = "eng")
  {
    var series = await this.GetAsync<Series>(
      $"https://api4.thetvdb.com/v4/series/{id}/extended");

    if (series.SupportedTranslations.Contains(language))
    {
      var translation = await this.GetAsync<Translation>(
        $"https://api4.thetvdb.com/v4/series/{id}/translations/{language}");

      return series with {
        Overview = translation.Overview ?? series.Overview,
        Seasons = [.. series.Seasons.Select(
          i => i with {
            SupportedTranslations = [.. i.SupportedTranslations.SelectMany(i => i.Split(','))]
          }
        )]
      };
    }

    return series;
  }

  public async Task<Season> GetSeasonAsync(
    long id,
    string language = "eng")
  {
    var season = await this.GetAsync<Season>(
      $"https://api4.thetvdb.com/v4/seasons/{id}/extended");

    season = season with {
      SupportedTranslations = [.. season.SupportedTranslations.SelectMany(i => i.Split(','))]
    };

    if (season.SupportedTranslations.Contains(language))
    {
      var translation = await this.GetAsync<Translation>(
        $"https://api4.thetvdb.com/v4/seasons/{id}/translations/{language}");

      season = season with {
        Overview = translation.Overview ?? season.Overview
      };
    }

    return season;
  }

  public async Task<Episode> GetEpisodeAsync(
    long id,
    string language = "eng")
  {
    var episode = await this.GetAsync<Episode>(
      $"https://api4.thetvdb.com/v4/episodes/{id}/extended");

    if (episode.SupportedTranslations.Contains(language))
    {
      var translation = await this.GetAsync<Translation>(
        $"https://api4.thetvdb.com/v4/episodes/{id}/translations/{language}");

      episode = episode with {
        Overview = translation.Overview ?? episode.Overview
      };
    }

    return episode;
  }

  public async Task<byte[]> DownloadArtworkAsync(
    Artwork artwork)
  {
    return await this.httpClient.GetByteArrayAsync(artwork.Image);
  }
}
