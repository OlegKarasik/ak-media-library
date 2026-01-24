using System.ComponentModel;
using MediaLibrary.Business;
using MediaLibrary.Business.Navigation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MediaLibrary.Commands;

public class EnrichCommandSettings : CommandSettings
{
  private const string DEFAULT_LANGUAGE = "eng";
  private const int DEFAULT_FUZZY_MATCH = 10;

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

  [CommandOption("-c|--language-code")]
  [DefaultValue(DEFAULT_LANGUAGE)]
  public required string Language
  {
    get; set;
  }

  [CommandOption("-f|--fuzzy-match")]
  [DefaultValue(DEFAULT_FUZZY_MATCH)]
  public required int FuzzyMatch
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
