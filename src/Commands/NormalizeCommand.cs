using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MediaLibrary.Business;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommand : AsyncCommand<NormalizeCommandSettings>
{
  public override Task<int> ExecuteAsync(
    CommandContext context, 
    NormalizeCommandSettings settings)
  {
    LibraryItem library;
    using (var fs = File.OpenRead(new IndexPath(settings.LibraryPath).Value)) 
    {
      var result = JsonSerializer.Deserialize<LibraryItem>(
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
    switch (Navigator.GetItem(library, new NavigationQuery(settings.IndexPath))) 
    {

    }
      
    return Task.FromResult(0);
  }
}
