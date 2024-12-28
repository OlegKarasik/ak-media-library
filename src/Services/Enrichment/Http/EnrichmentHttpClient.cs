using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediaLibrary.Extensions.Services.Enrichment.Http.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace MediaLibrary.Extensions.Services.Enrichment.Http;

public partial class EnrichmentHttpClient
{
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

    private readonly HttpClient client;

    private readonly IConfigurationRoot configuration;

    public AuthorizationTokenSource(
      HttpClient client,
      IConfigurationRoot configuration)
    {
      this.client = client ?? throw new ArgumentNullException(nameof(client));
      this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<string?> GetAccessToken(
      CancellationToken cancellationToken)
    {
      var result = await this.client.PostAsJsonAsync(
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
  
  private readonly HttpClient client;

  private readonly EnrichmentHttpCache cache;

  public EnrichmentHttpClient(
    HttpClient client,
    EnrichmentHttpCache cache)
  {
    this.client = client ?? throw new ArgumentNullException(nameof(client));
    this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
  }

  private async Task<T> GetAsync<T>(
    string uri)

    where T : class
  {
    var response = await this.cache.GetResponseAsync(uri);
    if (string.IsNullOrEmpty(response))
    {
      response = await this.client.GetStringAsync(uri);
      
      await this.cache.SaveResponseAsync(uri, response);
    }

    var output = JsonSerializer.Deserialize<GenericResponse<T>>(response, JsonSerializerOptions.Web);
    return output!.Data;
  }

  private async Task<Search[]?> SearchAsync(
    string title,
    string type,
    string language = "eng")
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
    }

    var results = await this.GetAsync<Search[]>(
      $"https://api4.thetvdb.com/v4/search?query={title}&type={type}&language={language}");

    results = [.. results.Select(
      result => result with {
        NameTranslations = result.NameTranslations ?? [],
        OverviewTranslations = result.OverviewTranslations ?? []
      })];

    return [.. results.Select(
      result => result with {
        Name = result.NameTranslations.TryGetValue(language, out var name) ? name : result.Name,
        Overview = result.OverviewTranslations.TryGetValue(language, out var overview) ? overview : result.Overview
      })];
  }

  public async Task<Search[]> SearchSeriesAsync(
    string title,
    string language)
  {
    return await this.SearchAsync(title, "series", language) ?? [];
  }

  public async Task<Series> GetSeriesAsync(
    long id,
    string language = "eng")
  {
    var series = await this.GetAsync<Series>(
      $"https://api4.thetvdb.com/v4/series/{id}/extended?meta=episodes");

    series = series with {
      Genres = series.Genres ?? [],
      Seasons = series.Seasons ?? [],
      Episodes = series.Episodes ?? [],
      Characters = series.Characters ?? [],
      Artworks = series.Artworks ?? [],
      NameTranslations = series.NameTranslations ?? [],
      OverviewTranslations = series.OverviewTranslations ?? []
    };
    
    series = series with {
      Seasons = [.. series.Seasons.Select(
        i => i with {
          OverviewTranslations = [.. i.OverviewTranslations.SelectMany(i => i.Split(','))]
        }
      )],
      Artworks = [.. series.Artworks.Where(
        i => i.IncludesText == false || i.Language == language || i.Language is null
      )]
    };
    series = series with {
      Seasons = [.. ProcessSeasons(series.Seasons, language)]
    };

    if (series.NameTranslations.Contains(language) || series.OverviewTranslations.Contains(language))
    {
      var translation = await this.GetAsync<Translation>(
        $"https://api4.thetvdb.com/v4/series/{id}/translations/{language}");

      return series with {
        Name = translation.Name ?? series.Name,
        Overview = translation.Overview ?? series.Overview
      };
    }

    return series;

    static IEnumerable<Series.Season> ProcessSeasons(
      IEnumerable<Series.Season> seasons,
      string language)
    {
      foreach (var group in seasons.GroupBy(i => i.Index))
      {
        yield return group
          .Where(i => i.OverviewTranslations.Contains(language))
          .FirstOrDefault() ?? group.First();
      }
    }
  }

  public async Task<Season> GetSeasonAsync(
    long id,
    string language = "eng")
  {
    var season = await this.GetAsync<Season>(
      $"https://api4.thetvdb.com/v4/seasons/{id}/extended");

    season = season with {
      Artworks = season.Artworks ?? [],
      NameTranslations = season.NameTranslations ?? [],
      OverviewTranslations = season.OverviewTranslations ?? []
    };

    season = season with {
      NameTranslations = [.. season.NameTranslations.SelectMany(i => i.Split(','))],
      OverviewTranslations = [.. season.OverviewTranslations.SelectMany(i => i.Split(','))]
    };

    if (season.NameTranslations.Contains(language) || season.OverviewTranslations.Contains(language))
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

    episode = episode with {
      Characters = episode.Characters ?? [],
      NameTranslations = episode.NameTranslations ?? [],
      OverviewTranslations = episode.OverviewTranslations ?? []
    };

    if (episode.NameTranslations.Contains(language) || episode.OverviewTranslations.Contains(language))
    {
      var translation = await this.GetAsync<Translation>(
        $"https://api4.thetvdb.com/v4/episodes/{id}/translations/{language}");

      episode = episode with {
        Name = translation.Name ?? episode.Name,
        Overview = translation.Overview ?? episode.Overview
      };
    }

    return episode;
  }

  public async Task<byte[]> GetArtworkAsync(
    Artwork artwork)
  {
    return await this.client.GetByteArrayAsync(artwork.Image);
  }
}
