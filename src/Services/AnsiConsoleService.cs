using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MediaLibrary.Extensions.Services;

public static class AnsiConsoleExtensions
{
  private class EscapeAnsiConsoleInput : IAnsiConsoleInput
  {
    private readonly IAnsiConsoleInput original;

    public EscapeAnsiConsoleInput(
      IAnsiConsoleInput original)
    {
      ArgumentNullException.ThrowIfNull(original);

      this.original = original;
    }

    public bool IsKeyAvailable()
    {
      return this.original.IsKeyAvailable();
    }

    public ConsoleKeyInfo? ReadKey(
      bool intercept)
    {
      var key = this.original.ReadKey(intercept);
      if (key.HasValue && key.Value.Key == ConsoleKey.Escape)
      {
        throw new OperationCanceledException();
      }
      return key;
    }

    public async Task<ConsoleKeyInfo?> ReadKeyAsync(
      bool intercept, 
      CancellationToken cancellationToken)
    {
      var key = await this.original.ReadKeyAsync(intercept, cancellationToken);
      if (key.HasValue && key.Value.Key == ConsoleKey.Escape)
      {
        throw new OperationCanceledException();
      }
      return key;
    }
  }

  private class EscapeAnsiConsole : IAnsiConsole
  {
    private readonly IAnsiConsole original;
    private readonly IAnsiConsoleInput input;

    public Profile Profile => this.original.Profile;

    public IAnsiConsoleCursor Cursor => this.original.Cursor;

    public IAnsiConsoleInput Input => this.input;

    public IExclusivityMode ExclusivityMode => this.original.ExclusivityMode;

    public RenderPipeline Pipeline => this.original.Pipeline;

    public EscapeAnsiConsole(
      IAnsiConsole original)
    {
      ArgumentNullException.ThrowIfNull(original);
      
      this.original = original;
      this.input = new EscapeAnsiConsoleInput(original.Input);
    }

    public void Clear(
      bool home)
    {
      this.original.Clear(home);
    }

    public void Write(
      IRenderable renderable)
    {
      this.original.Write(renderable);
    }
  }

  extension(AnsiConsole)
  {
    public static bool TryPrompt<T>(
      IPrompt<T> prompt,
      [NotNullWhen(true)] out T? result)
    {
      try
      {
        result = new EscapeAnsiConsole(AnsiConsole.Console).Prompt(prompt);
      }
      catch (OperationCanceledException)
      {
        result = default;
      }
      return result is not null;
    }    
  }
}


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
