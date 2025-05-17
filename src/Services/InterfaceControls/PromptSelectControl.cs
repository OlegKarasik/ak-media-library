using Spectre.Console;

namespace MediaLibrary.Extensions.Services.InterfaceContrls;

public class PromptSelectControl<T>
  where T : notnull
{
  public enum PromptMatches
  {
    Items,
    Commands,

    Item,
    Skip,
    Back,
    Next,
    Accept,
    Decline,
    Cancel,
    Yes,
    No
  }

  public abstract class PromptResult
  {
    public PromptMatches Match { get; }

    protected PromptResult(
      PromptMatches match)
    {
      this.Match = match;
    }
  }

  public class PromptItemResult : PromptResult
  {
    public T Item { get; }

    public PromptItemResult(
      T item)

      : base(PromptMatches.Item)
    {
      this.Item = item;
    }
  }

  public class PromptControlResult : PromptResult
  {
    public PromptControlResult(
      PromptMatches match)

      : base(match)
    {
    }
  }

  private readonly SelectionPrompt<PromptResult> prompt;

  private Func<T, string>? itemToString;
  private Func<PromptCommands, string>? commandToString;
  private string? itemGroupString;
  private string? commandGroupString;

  public PromptSelectControl(
    string title,
    IEnumerable<T> items)

    : this(title, items, [])
  {
  }

  public PromptSelectControl(
    string title,
    IEnumerable<PromptCommands> commands)

    : this(title, [], commands)
  {
  }

  public PromptSelectControl(
    string title,
    IEnumerable<T> items,
    IEnumerable<PromptCommands> commands)
  {
    ArgumentException.ThrowIfNullOrEmpty(title);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(commands);

    this.prompt = new SelectionPrompt<PromptResult>()
      .Title(title)
      .UseConverter(
        i =>
        {
          return i switch
          {
            PromptItemResult result => ResultToString(result),
            PromptControlResult result => ResultToString(result),
            _ => throw new NotImplementedException(),
          };
        });

    if (items.Any())
    {
      this.prompt.AddChoiceGroup(
        new PromptControlResult(PromptMatches.Items),
        items.Select(i => new PromptItemResult(i)));
    }
    if (commands.Any())
    {
      this.prompt.AddChoiceGroup(
        new PromptControlResult(PromptMatches.Commands),
        commands.Select(i => new PromptControlResult(this.Convert(i))));
    }
  }

  private string ResultToString(
    PromptItemResult result)
  {
    return this.itemToString is not null ? this.itemToString(result.Item) : $"{result.Item}";
  }

  private string ResultToString(
    PromptControlResult result)
  {
    switch (result.Match)
    {
      case PromptMatches.Items:
        if (this.itemGroupString is not null)
        {
          return this.itemGroupString;
        }
        break;
      case PromptMatches.Commands:
        if (this.commandGroupString is not null)
        {
          return this.commandGroupString;
        }
        break;
    }
    return this.commandToString is not null ? this.commandToString(this.Convert(result.Match)) : $"{result.Match}";
  }

  private PromptMatches Convert(
    PromptCommands v)
  {
    return v switch
    {
      PromptCommands.Skip => PromptMatches.Skip,
      PromptCommands.Back =>  PromptMatches.Back,
      PromptCommands.Next => PromptMatches.Next,
      PromptCommands.Accept => PromptMatches.Accept,
      PromptCommands.Decline => PromptMatches.Decline,
      PromptCommands.Cancel => PromptMatches.Cancel,
      PromptCommands.Yes => PromptMatches.Yes,
      PromptCommands.No => PromptMatches.No,
      _ => throw new NotImplementedException(),
    };
  }

  private PromptCommands Convert(
    PromptMatches v)
  {
    return v switch
    {
      PromptMatches.Skip => PromptCommands.Skip,
      PromptMatches.Back => PromptCommands.Back,
      PromptMatches.Next => PromptCommands.Next,
      PromptMatches.Accept => PromptCommands.Accept,
      PromptMatches.Decline => PromptCommands.Decline,
      PromptMatches.Cancel => PromptCommands.Cancel,
      PromptMatches.Yes => PromptCommands.Yes,
      PromptMatches.No => PromptCommands.No,
      _ => throw new NotImplementedException(),
    };
  }

  public PromptSelectControl<T> UseItemsGroupString(
    string value)
  {
    ArgumentException.ThrowIfNullOrEmpty(value);

    this.itemGroupString = value;

    return this;
  }

  public PromptSelectControl<T> UseCommandsGroupString(
    string value)
  {
    ArgumentException.ThrowIfNullOrEmpty(value);

    this.commandGroupString = value;

    return this;
  }

  public PromptSelectControl<T> UseItemString(
    Func<T, string> toString)
  {
    this.itemToString = toString ?? throw new ArgumentNullException(nameof(toString));

    return this;
  }

  public PromptSelectControl<T> UseCommandString(
    Func<PromptCommands, string> toString)
  {
    this.commandToString = toString ?? throw new ArgumentNullException(nameof(toString));

    return this;
  }

  public IPrompt<PromptResult> GetPrompt()
  {
    return this.prompt;
  }
}
