using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Enrichment.Http.Models;

public record class Series
{
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

    [JsonPropertyName("seasonNumber")]
    public required long SeasonIndex
    {
      get; init;
    }

    [JsonPropertyName("nameTranslations")]
    public string[] NameTranslations
    {
      get; init;
    }

    [JsonPropertyName("overviewTranslations")]
    public string[] OverviewTranslations
    {
      get; init;
    }

    public Episode()
    {
      this.NameTranslations = [];
      this.OverviewTranslations = [];
    }
  }

  public record class SeasonType
  {
    [JsonPropertyName("type")]
    public required string Value
    {
      get; init;
    }
  }

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

    [JsonPropertyName("type")]
    public required SeasonType Type
    {
      get; init;
    }

    [JsonPropertyName("overviewTranslations")]
    public string[] OverviewTranslations
    {
      get; init;
    }

    public Season()
    {
      this.OverviewTranslations = [];
    }
  }

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

  [JsonPropertyName("firstAired")]
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

  [JsonPropertyName("episodes")]
  public Episode[] Episodes
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

  [JsonPropertyName("nameTranslations")]
  public string[] NameTranslations
  {
    get; init;
  }

  [JsonPropertyName("overviewTranslations")]
  public string[] OverviewTranslations
  {
    get; init;
  }

  public Series()
  {
    this.Genres = [];
    this.Seasons = [];
    this.Episodes = [];
    this.Characters = [];
    this.Artworks = [];
    this.NameTranslations = [];
    this.OverviewTranslations = [];
  }
}
