using MediaLibrary.Business.Enrichment.Common;
using MediaLibrary.Business.Enrichment.Http;
using MediaLibrary.Business.Enrichment.Models;

namespace MediaLibrary.Business.Enrichment;

public class EnrichmentService
{
  private readonly EnrichmentHttpClient client;

  public EnrichmentService(
    EnrichmentHttpClient client)
  {
    this.client = client ?? throw new ArgumentNullException(nameof(client));
  }

  public async Task<Search[]> SearchSeriesAsync(
    string title,
    string language = "eng")
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
    }

    var result = await this.client.SearchSeriesAsync(title, language);
    return [.. result.Select(i => new Search{
      Id = i.Id,
      Title = new MediaTitle(i.Name),
      Overview = new MediaOverview(i.Overview),
      Year = i.Year
    })];
  }

  public async Task<Series> GetSeriesAsync(
    long id,
    string language = "eng")
  {
    var series = await this.client.GetSeriesAsync(id, language);

    var seasons = await series.Seasons
      .ToAsyncEnumerable()
      .SelectAwait(async i => await this.client.GetSeasonAsync(i.Id, language))
      .ToArrayAsync();

    var episodes = await series.Episodes
      .ToAsyncEnumerable()
      .SelectAwait(
        async i => 
        {
          var episode = await this.client.GetEpisodeAsync(i.Id, language);
          return episode with {
            SeasonIndex = episode.SeasonIndex ?? i.SeasonIndex
          };
        })
      .ToArrayAsync();

    var result =  new Series
    {
      Title = new MediaTitle(series.Name),
      Overview = new MediaOverview(series.Overview),
      Date = series.Date,
      Year = series.Year,
      Genres = [.. series.Genres.Select(i => i.Name)],
      Seasons = seasons.ToDictionary(
        i => i.Index, 
        i => new Season
        {
          Index = i.Index,
          Overview = new MediaOverview(i.Overview),
          Episodes = episodes
            .Where(j => j.Kind == Http.Models.EpisodeKind.Episode)
            .Where(j => j.SeasonIndex == i.Index || j.SeasonIndex == 0)
            .ToDictionary(
              j => j.Name,
              j => new Episode
              {
                Title = j.Name,
                SeasonIndex = i.Index,
                Date = j.Date,
                Overview = j.Overview,
                Directors = [.. j.Characters.Where(x => x.PersonType == "Director").Select(x => new Director { Name = x.PersonName })],
                Writers = [.. j.Characters.Where(x => x.PersonType == "Writer").Select(x => new Writer { Name = x.PersonName })],
              },
              EpisodeTitleEqualityComparer.Default)
        })
    };

    result = result with {
      Image = await GetArtwork(series.Artworks, Http.Models.ArtworkKind.SeriesPoster, language),
      ImageBackground = await GetArtwork(series.Artworks, Http.Models.ArtworkKind.SeriesBackground, language),
    };
    foreach (var (index, resultSeason) in result.Seasons)
    {
      var season = seasons.First(i => i.Index == index);
      result.Seasons[index] = resultSeason with {
        Image = await GetArtwork(season.Artworks, Http.Models.ArtworkKind.SeasonPoster, language),
        ImageBackground = await GetArtwork(season.Artworks, Http.Models.ArtworkKind.SeasonBackground, language),
      };
    }

    return result;

    async Task<byte[]?> GetArtwork(
      IEnumerable<Enrichment.Http.Models.Artwork> artworks,
      Enrichment.Http.Models.ArtworkKind kind,
      string language)
    {
      var artwork = artworks
        .Where(
          i =>  i.Kind == kind 
            && (i.IncludesText == false || i.Language == language || i.Language is null))
        .OrderByDescending(i => i.Score)
        .FirstOrDefault();
      
      if (artwork is not null)
      {
        return await this.client.GetArtworkAsync(artwork);
      }
      return null;
    }
  }
}
