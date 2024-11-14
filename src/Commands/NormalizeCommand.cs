using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommand : AsyncCommand<NormalizeCommandSettings>
{
  public override Task<int> ExecuteAsync(
    CommandContext context, 
    NormalizeCommandSettings settings)
  {
    return Task.FromResult(0);
  }
}
