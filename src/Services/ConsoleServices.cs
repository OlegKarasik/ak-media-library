using Spectre.Console;

namespace MediaLibrary.Extensions.Services;

public class ConsoleServices
{
  public static bool YesNoConfirmation(
    string question)
  {
    var prompt = new SelectionPrompt<string>()
      .Title(question ?? "Confirm")
      .AddChoices("Yes", "No");

    return AnsiConsole.Prompt(prompt) switch {
      "Yes" => true,
       "No" => false,
          _ => throw new NotSupportedException()
    };
  }
}
