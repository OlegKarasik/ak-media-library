using Spectre.Console.Cli;

using MediaLibrary.Commands;
using MediaLibrary.Extensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
  .AddUserSecrets<EnrichCommand>()
  .Build();

var services = new ServiceCollection();

services
  .AddSingleton<IConfigurationRoot>(configuration)
  .AddTransient<EnrichmentService.AuthorizationTokenSource>()
  .AddTransient<EnrichmentService.Authorization>();

services
  .AddHttpClient<EnrichmentService>()
  .AddHttpMessageHandler<EnrichmentService.Authorization>();

var app = new CommandApp(new TypeRegistrationService(services));
app.Configure(
  o =>
  {
    o.AddCommand<ScanCommand>("scan");
    o.AddCommand<InfoCommand>("info");
    o.AddCommand<EnrichCommand>("enrich");
  });

return await app.RunAsync(args);
