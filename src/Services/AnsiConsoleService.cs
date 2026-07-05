using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MediaLibrary.Extensions.Services;

public static class AnsiConsoleExtensions
{
  private class EscapeAnsiConsoleInput : IAnsiConsoleInput
  {
    private readonly IAnsiConsoleInput original;
    private readonly CancellationTokenSource cts;

    public EscapeAnsiConsoleInput(
      IAnsiConsoleInput original,
      CancellationTokenSource cts)
    {
      ArgumentNullException.ThrowIfNull(original);

      this.original = original;
      this.cts = cts;
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
        cts.Cancel();
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
        await cts.CancelAsync();
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
      IAnsiConsole original,
      CancellationTokenSource cts)
    {
      ArgumentNullException.ThrowIfNull(original);
      
      this.original = original;
      this.input = new EscapeAnsiConsoleInput(original.Input, cts);
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
      var promptTop = Console.CursorTop;
      try
      {
        var cts = new CancellationTokenSource();
        result = new EscapeAnsiConsole(AnsiConsole.Console, cts).PromptAsync(prompt, cts.Token).GetAwaiter().GetResult();
      }
      catch (OperationCanceledException)
      {
        // Here we erase the prompt rendered by Spectre console to
        // ensure the same user experience regardless of whether the prompt
        // has been cancelled or not.
        //
        var promptBottom = Console.CursorTop;
        var promptErasure = new string(' ', Console.BufferWidth);
        for (int i = promptTop; i <= promptBottom; i++)
        {
          Console.SetCursorPosition(0, i);
          Console.Write(promptErasure);
        }
        Console.SetCursorPosition(0, promptTop);

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
