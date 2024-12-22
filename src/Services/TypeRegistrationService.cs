using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

namespace MediaLibrary.Extensions.Services;

public class TypeRegistrationService : ITypeRegistrar
{
  private readonly IServiceCollection services;

  public TypeRegistrationService(
    IServiceCollection services)
  {
    this.services = services ?? throw new ArgumentNullException(nameof(services));
  }

  public ITypeResolver Build()
  {
    return new TypeResolutionService(this.services.BuildServiceProvider());
  }

  public void Register(
    Type service, 
    Type implementation)
  {
    this.services.AddSingleton(service, implementation);
  }

  public void RegisterInstance(
    Type service, 
    object implementation)
  {
   this.services.AddSingleton(service, implementation);
  }

  public void RegisterLazy(
    Type service, 
    Func<object> factory)
  {
    this.services.AddSingleton(service, provider => factory());
  }
}
