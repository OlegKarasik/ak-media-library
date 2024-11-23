using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using MediaLibrary.Business.Items;
using MediaLibrary.Business.Navigation;
using MediaLibrary.Extensions;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;
public partial class InfoCommand : AsyncCommand<InfoCommandSettings>
{
  private readonly InfoCommandOptions options;

  public InfoCommand()
  {
    this.options = new InfoCommandOptions();
  }

  public override Task<int> ExecuteAsync(
    CommandContext context, 
    InfoCommandSettings settings)
  {
    IndexItem library;
    using (var fs = File.OpenRead(settings.Index.Value)) 
    {
      var result = JsonSerializer.Deserialize<IndexItem>(
        fs,
        new JsonSerializerOptions
        {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
