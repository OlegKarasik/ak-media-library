using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommandSettings : CommandSettings
{
  [CommandArgument(0, "<library-path>")]
  public string LibraryPath
  {
    get; set; 
  }

  [CommandArgument(0, "<index-path>")]
  public string IndexPath
  {
    get; set; 
  }

  public NormalizeCommandSettings()
  {
    this.LibraryPath = string.Empty;
    this.IndexPath = string.Empty;
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
