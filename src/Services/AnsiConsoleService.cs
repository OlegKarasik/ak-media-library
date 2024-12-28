using MediaLibrary.Extensions.Services.Enrichment.Models;
using Spectre.Console;

namespace MediaLibrary.Extensions.Services;

public class AnsiConsoleService
{
  public enum ContinueBack
  {
    Continue,
    Back
  }

  public enum ContinueBackSkip
  {
    Continue,
    Back,
    Skip
  }

  public static void Rule(
    string value)
  {
    if (string.IsNullOrEmpty(value))
    {
      throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
    }

    AnsiConsole.Write(
      new Rows(
        new Text(string.Empty),
        new Rule($"[Yellow]{value}[/]").LeftJustified()));
  }

  public static T Select<T>(
    IEnumerable<T> items,
    Func<T, string> converter)

    where T: notnull
  {
    if (items is null)
    {
      throw new ArgumentNullException(nameof(items));
    }
    if (converter is null)
    {
      throw new ArgumentNullException(nameof(converter));
    }

    var selection = AnsiConsole.Prompt(
      new SelectionPrompt<T>()
        .Title(string.Empty)
        .UseConverter(converter)
        .AddChoices(items));

    AnsiConsole.MarkupLineInterpolated($"Proceed with [Green]{converter(selection)}[/]");

    return selection;
  }

  public static bool Question(
    string question)
  {
    AnsiConsole.MarkupLine(question);

    var convertion = new Func<bool, string>(i => i switch { true => "Yes", false => "No" });
    var prompt = new SelectionPrompt<bool>()
      .Title(string.Empty)
      .UseConverter(convertion)
      .AddChoices(true, false);

    var result = AnsiConsole.Prompt(prompt);

    AnsiConsole.MarkupLineInterpolated($"[Blue]{convertion(result)}[/]");

    return result;
  }

  public static bool SelectYesNo()
  {
    var prompt = new SelectionPrompt<string>()
      .Title(string.Empty)
      .AddChoices(["Yes", "No"]);

    switch (AnsiConsole.Prompt(prompt))
    {
      case "Yes":
        AnsiConsole.MarkupLine("Yes");
        return true;
      case "No":
        AnsiConsole.MarkupLine("No");
        return false;
      default:
        throw new NotSupportedException();
    }
  }

  public static ContinueBack SelectContinueBack()
  {
    var prompt = new SelectionPrompt<ContinueBack>()
      .Title(string.Empty)
      .AddChoices(
        ContinueBack.Continue, 
        ContinueBack.Back);

    switch (AnsiConsole.Prompt(prompt))
    {
      case ContinueBack.Continue:
        AnsiConsole.MarkupLine("Continue");
        return ContinueBack.Continue;
      case ContinueBack.Back:
        AnsiConsole.MarkupLine("Back");
        return ContinueBack.Back;
      default:
        throw new NotSupportedException();
    }
  }

  public static ContinueBackSkip SelectContinueBackSkip()
  {
    var prompt = new SelectionPrompt<ContinueBackSkip>()
      .Title(string.Empty)
      .AddChoices(
        ContinueBackSkip.Continue, 
        ContinueBackSkip.Back, 
        ContinueBackSkip.Skip);

    switch (AnsiConsole.Prompt(prompt))
    {
      case ContinueBackSkip.Continue:
        AnsiConsole.MarkupLine("Continue");
        return ContinueBackSkip.Continue;
      case ContinueBackSkip.Back:
        AnsiConsole.MarkupLine("Back");
        return ContinueBackSkip.Back;
      case ContinueBackSkip.Skip:
        AnsiConsole.MarkupLine("Back");
        return ContinueBackSkip.Skip;
      default:
        throw new NotSupportedException();
    }
  }
}
