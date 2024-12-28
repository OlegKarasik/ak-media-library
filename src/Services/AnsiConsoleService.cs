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
    ArgumentNullException.ThrowIfNull(value);

    AnsiConsole.Write(
      new Rows(
        new Text(string.Empty),
        new Rule($"[Yellow]{value}[/]").LeftJustified()));
  }

  public static T SelectOneOf<T>(
    IEnumerable<T> items,
    Func<T, string> converter)

    where T: notnull
  {
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(converter);

    var selection = AnsiConsole.Prompt(
      new SelectionPrompt<T>()
        .Title(string.Empty)
        .UseConverter(converter)
        .AddChoices(items));

    return selection;
  }

  public static bool SelectYesOrNo()
  {
    var convertion = new Func<bool, string>(i => i switch { true => "Yes", false => "No" });
    var prompt = new SelectionPrompt<bool>()
      .Title(string.Empty)
      .UseConverter(convertion)
      .AddChoices(true, false);

    var result = AnsiConsole.Prompt(prompt);

    AnsiConsole.MarkupLineInterpolated($"{convertion(result)}");

    return result;
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
