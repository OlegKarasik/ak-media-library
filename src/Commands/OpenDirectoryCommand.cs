using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class OpenDirectoryCommand : AsyncCommand<OpenDirectoryCommandSettings>
{
  public override Task<int> ExecuteAsync(
    CommandContext context, 
    OpenDirectoryCommandSettings settings)
  {
    return Task.FromResult(0);
  }
}