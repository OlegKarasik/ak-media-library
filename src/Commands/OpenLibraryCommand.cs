using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class OpenLibraryCommand : AsyncCommand<OpenLibraryCommandSettings>
{
  public override Task<int> ExecuteAsync(
    CommandContext context, 
    OpenLibraryCommandSettings settings)
  {
    return Task.FromResult(0);
  }
}
