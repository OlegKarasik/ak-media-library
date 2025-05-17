using Spectre.Console;
using Spectre.Console.Rendering;

namespace MediaLibrary.Extensions.Services.InterfaceContrls;

public class VisualPanelControl
{
  private readonly Panel panel;

  public VisualPanelControl(
    string title,
    string content)
  {
    ArgumentException.ThrowIfNullOrEmpty(title);
    ArgumentException.ThrowIfNullOrEmpty(content);
    
    this.panel = new Panel(new Text(content))
      .Header(title.ToUpper(), Justify.Left);
  }

  public IRenderable GetRenderable()
  {
    return this.panel;
  }
}
