using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class ScanLibraryCommandSettings : CommandSettings
{
  [CommandArgument(0, "<library-path>")]
  public string LibraryPath
  {
    get; set; 
  }

  public ScanLibraryCommandSettings()
  {
    this.LibraryPath = string.Empty;
  }

  public override ValidationResult Validate()
  {
    if (!Directory.Exists(this.LibraryPath))
    {
      return ValidationResult.Error($"Invalid path \"{this.LibraryPath}\"");
    }

    return base.Validate();
  }
}
