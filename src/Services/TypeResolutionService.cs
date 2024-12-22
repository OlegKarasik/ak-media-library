using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

namespace MediaLibrary.Extensions.Services;

public class TypeResolutionService : ITypeResolver, IDisposable
{
  private readonly IServiceProvider provider;

  public TypeResolutionService(
    IServiceProvider provider)
  {
    this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
  }

  public object? Resolve(
    Type? type)
  {
    return type is not null 
      ? this.provider.GetRequiredService(type)
      : null;
  }

  public void Dispose()
  {
    if (this.provider is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
}
