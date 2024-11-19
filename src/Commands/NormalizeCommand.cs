using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommand : AsyncCommand<NormalizeCommandSettings>
{
  private class NormalizationData
  {
    public virtual string EpisodePosition => "";

    public virtual string EpisodeSpanPositionStart => "";
    
    public virtual string EpisodeSpanPositionEnd => "";
    
    public virtual string EpisodeTitle => "";
    
    public virtual string SeasonPosition => "";
    
    public virtual string SeasonSpanPositionStart => "";
    
    public virtual string SeasonSpanPositionEnd => "";

    public virtual string SeasonTitle => "";
    
    public virtual string ShowTitle => "";
    
    public virtual string MovieTitle => "";
  }

  private class EpisodeNormalizationData : NormalizationData
  {
    private readonly EpisodeItem episode;

    public override string EpisodeTitle => episode.Title;

    public override string EpisodePosition => episode.EpisodePosition.Value.ToString();

    public override string EpisodeSpanPositionStart => episode.EpisodePosition.ValueStart.ToString();

    public override string EpisodeSpanPositionEnd => episode.EpisodePosition.ValueEnd.ToString();

    public override string SeasonPosition => episode.SeasonPosition.Value.ToString();

    public override string SeasonSpanPositionStart => episode.SeasonPosition.ValueStart.ToString();

    public override string SeasonSpanPositionEnd => episode.SeasonPosition.ValueEnd.ToString();

    public EpisodeNormalizationData(
      EpisodeItem item)
    {
      this.episode = item ?? throw new ArgumentNullException(nameof(item));
    }
  }

  private readonly NormalizeCommandOptions options;

  public NormalizeCommand()
  {
    this.options = new NormalizeCommandOptions();
  }

  private string UnwrapNormalizationPattern(
    string pattern,
    NormalizationData data)
  {
    return Regex.Replace(
      pattern, 
      "{.+?}", 
      match => {
        return match.Value switch
        {
          ItemMatchConstants.EPISODE_POSITION => data.EpisodePosition,
          ItemMatchConstants.EPISODE_SPAN_POSITION_START => data.EpisodeSpanPositionStart,
          ItemMatchConstants.EPISODE_SPAN_POSITION_END => data.EpisodeSpanPositionEnd,
          ItemMatchConstants.EPISODE_TITLE => data.EpisodeTitle,
          ItemMatchConstants.SEASON_POSITION => data.SeasonPosition,
          ItemMatchConstants.SEASON_SPAN_POSITION_START => data.SeasonSpanPositionStart,
          ItemMatchConstants.SEASON_SPAN_POSITION_END => data.SeasonSpanPositionEnd,
          ItemMatchConstants.SEASON_TITLE => data.SeasonTitle,
          ItemMatchConstants.SHOW_TITLE => data.ShowTitle,
          ItemMatchConstants.MOVIE_TITLE => data.MovieTitle,
          _ => throw new NotImplementedException(),
        };
      });
  }

  private string GetNormalizatinPattern(
    EpisodeItem episode)
  {
    if (episode.SeasonPosition.IsSpanning)
    {
      if (episode.EpisodePosition.IsSpanning)
      {
        return this.options.EpisodeSeasonRangePattern;
      }
      throw new NotSupportedException();
    }
    if (episode.EpisodePosition.IsSpanning)
    {
      return this.options.EpisodeRangePattern;
    }
    return this.options.EpisodePattern;
  }

  public override Task<int> ExecuteAsync(
    CommandContext context, 
    NormalizeCommandSettings settings)
  {
    IndexItem library;
    using (var fs = File.OpenRead(settings.Index.Value)) 
    {
      var result = JsonSerializer.Deserialize<IndexItem>(
        fs,
        new JsonSerializerOptions
        {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        });

      if (result is null)
      {
        throw new Exception();
      }
      library = result;
    }
    switch (IndexSearch.GetItem(library, settings.IndexQuery)) 
    {
      case EpisodeItem episode:
        {
          this.UnwrapNormalizationPattern(
            this.GetNormalizatinPattern(episode),
            new EpisodeNormalizationData(episode));
        }
        break;

    }
      
    return Task.FromResult(0);
  }
}
