using Spectre.Console.Cli;

using MediaLibrary.Commands;
using MediaLibrary.Extensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MediaLibrary.Business.Enrichment;
using MediaLibrary.Business.Enrichment.Http;

var configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddUserSecrets<EnrichCommand>()
  .Build();

var services = new ServiceCollection();

services
  .AddSingleton(configuration)
  .AddTransient<EnrichmentService>()
  .AddTransient<EnrichmentHttpCache>()
  .AddTransient<EnrichmentHttpClient.AuthorizationTokenSource>()
  .AddTransient<EnrichmentHttpClient.Authorization>();

services
  .Configure<EnrichmentHttpCacheOptions>(configuration.GetSection("EnrichmentHttpCache"));

services
  .AddHttpClient<EnrichmentHttpClient>()
  .AddHttpMessageHandler<EnrichmentHttpClient.Authorization>();

var app = new CommandApp(new TypeRegistrationService(services));
app.Configure(
  o =>
  {
    o.AddCommand<ScanCommand>("scan");
    o.AddCommand<EnrichCommand>("enrich");
  });

return await app.RunAsync(args);
