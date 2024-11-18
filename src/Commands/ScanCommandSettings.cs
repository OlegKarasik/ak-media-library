using MediaLibrary.Business;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class ScanCommandSettings : CommandSettings
{
  [CommandArgument(0, "<directory>")]
  public required DirectoryPath Directory
  {
    get; init; 
  }

  public override ValidationResult Validate()
  {
    if (!System.IO.Directory.Exists(this.Directory.Value))
    {
      return ValidationResult.Error(
        $"Can't find directory \"{this.Directory}\", please ensure directory exists");
    }

    return base.Validate();
  }
}
