using System.ComponentModel;
using MediaLibrary.Business;
using MediaLibrary.Business.Navigation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class EnrichCommandSettings : CommandSettings
{
  private const int MATCH_ALLOWANCE_CONSTANT = 10;

  [CommandOption("-l|--library")]
  public required DirectoryPath Library
  {
    get; set; 
  }

  [CommandOption("-r|--request")]
  public required IndexSearchRequest SearchRequest
  {
    get; set;
  }

  [CommandOption("-a|--match-allowance")]
  [DefaultValue(MATCH_ALLOWANCE_CONSTANT)]
  public required long MatchAllowance
  {
    get; set;
  }

  public override ValidationResult Validate()
  {
    this.Library ??= new DirectoryPath(Environment.CurrentDirectory);
    this.SearchRequest ??= new IndexSearchRequest(IndexSearchRoot.Movies.ToString());

    if (!Directory.Exists(this.Library.Value))
    {
      return ValidationResult.Error(
        $"Directory \"{this.Library}\" doesn't exist, please ensure directory exists");
    }

    return base.Validate();
  }
}
