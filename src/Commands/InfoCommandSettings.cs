using MediaLibrary.Business;
using MediaLibrary.Business.Navigation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class InfoCommandSettings : CommandSettings
{
  [CommandOption("-l|--library")]
  public required DirectoryPath Library
  {
    get; init; 
  }

  [CommandOption("-q|--query")]
  public required IndexQuery IndexQuery
  {
    get; init; 
  }

  public override ValidationResult Validate()
  {
    if (!Directory.Exists(this.Library.Value))
    {
      return ValidationResult.Error(
        $"Directory \"{this.Library}\" doesn't exist, please ensure directory exists");
    }

    return base.Validate();
  }
}
