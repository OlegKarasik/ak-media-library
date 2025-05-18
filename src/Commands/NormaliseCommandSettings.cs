using MediaLibrary.Business;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormaliseCommandSettings : CommandSettings
{
  [CommandOption("-l|--library")]
  public required DirectoryPath Library
  {
    get; set; 
  }

  public override ValidationResult Validate()
  {
    this.Library ??= new DirectoryPath(Environment.CurrentDirectory);

    if (!Directory.Exists(this.Library.Value))
    {
      return ValidationResult.Error(
        $"Directory \"{this.Library}\" doesn't exist, please ensure directory exists");
    }

    return base.Validate();
  }
}
