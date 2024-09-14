using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class OpenDirectoryCommandSettings : CommandSettings
{
  [CommandArgument(0, "<directory>")]
  public string DirectoryPath
  {
    get; set; 
  }

  public OpenDirectoryCommandSettings()
  {
    this.DirectoryPath = string.Empty;
  }

  public override ValidationResult Validate()
  {
    if (!Directory.Exists(this.DirectoryPath))
    {
      return ValidationResult.Error($"Invalid path \"{this.DirectoryPath}\"");
    }

    return base.Validate();
  }
}
