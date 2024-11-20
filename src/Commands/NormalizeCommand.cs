using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Commands.Matching;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;
public partial class NormalizeCommand : AsyncCommand<NormalizeCommandSettings>
{
  private readonly NormalizeCommandOptions options;

  public NormalizeCommand()
  {
    this.options = new NormalizeCommandOptions();
  }

  private string EncodeItemMatch(
    string pattern,
    EncodeItemMatch match)
  {
    if (string.IsNullOrEmpty(pattern))
    {
      throw new ArgumentException($"'{nameof(pattern)}' cannot be null or empty.", nameof(pattern));
    }

    if (match is null)
    {
      throw new ArgumentNullException(nameof(match));
    }

    return NormalizeItemRegex().Replace(pattern, match.Encode);
  }

  [GeneratedRegex("{(?<Match>.+?)}")]
  private static partial Regex NormalizeItemRegex();

  private string GetEncodePattern(
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
          var result = this.EncodeItemMatch(
            this.GetEncodePattern(episode), 
            new EncodeEpisodeItemMatch(episode));
        }
        break;

    }
      
    return Task.FromResult(0);
  }
}
