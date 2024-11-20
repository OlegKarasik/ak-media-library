using System.Text.RegularExpressions;
using MediaLibrary.Business.Items;

namespace MediaLibrary.Commands;

public class EncodeEpisodeItemMatch : EncodeItemMatch
{
  private readonly EpisodeItem episode;

  protected override string EpisodeTitle => episode.Title;

  protected override string EpisodePosition => episode.EpisodePosition.Value.ToString();

  protected override string EpisodeSpanPositionStart => episode.EpisodePosition.ValueStart.ToString();

  protected override string EpisodeSpanPositionEnd => episode.EpisodePosition.ValueEnd.ToString();

  protected override string SeasonPosition => episode.SeasonPosition.Value.ToString();

  protected override string SeasonSpanPositionStart => episode.SeasonPosition.ValueStart.ToString();

  protected override string SeasonSpanPositionEnd => episode.SeasonPosition.ValueEnd.ToString();

  public EncodeEpisodeItemMatch(
    EpisodeItem item)
  {
    this.episode = item ?? throw new ArgumentNullException(nameof(item));
  }
}
