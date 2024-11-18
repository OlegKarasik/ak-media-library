using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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

    }
      
    return Task.FromResult(0);
  }
}
