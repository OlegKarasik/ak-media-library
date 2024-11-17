using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class NormalizeCommandSettings : CommandSettings
{
  [CommandArgument(0, "<library-path>")]
  public string LibraryPath
  {
    get; init; 
  }

  [CommandArgument(1, "<index-path>")]
  public string IndexPath
  {
    get; init; 
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
      return ValidationResult.Error($"The directory \"{this.LibraryPath}\" doesn't exist");
    }

    return base.Validate();
  }
}
