using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommand : AsyncCommand<NormalizeCommandSettings>
{
  public override Task<int> ExecuteAsync(
    CommandContext context, 
    NormalizeCommandSettings settings)
  {
    using (var fs = File.OpenRead(new IndexPath(settings.LibraryPath).Value)) 
    {
      var library = JsonSerializer.Deserialize<LibraryItem>(
        fs,
        new JsonSerializerOptions
        {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        });

      var item = Navigation.GetItem(library, new NavigationPath());
    }
      
    return Task.FromResult(0);
  }
}
