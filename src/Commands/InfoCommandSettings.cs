using MediaLibrary.Business;
using MediaLibrary.Business.Navigation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class InfoCommandSettings : CommandSettings
{
  [CommandArgument(0, "<index>")]
  public required IndexPath Index
  {
    get; init; 
  }

  [CommandArgument(1, "<index-query>")]
  public required IndexQuery IndexQuery
  {
    get; init; 
  }

  public override ValidationResult Validate()
  {
    if (!File.Exists(this.Index.Value))
    {
      return ValidationResult.Error(
        $"Can't find index \"{this.Index}\", please set first argument to be either path to the index or containing directory");
    }

    return base.Validate();
  }
}
