using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class ScanCommandSettings : CommandSettings
{
  [CommandArgument(0, "<library-path>")]
  public string LibraryPath
  {
    get; init; 
  }

  public ScanCommandSettings()
  {
    this.LibraryPath = string.Empty;
  }

  public override ValidationResult Validate()
  {
    if (!Directory.Exists(this.LibraryPath))
    {
      return ValidationResult.Error($"The directory \"{this.LibraryPath}\" doesn't exist");
    }

    return base.Validate();
  }
}
