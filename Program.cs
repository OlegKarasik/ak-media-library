using Spectre.Console.Cli;

using MediaLibrary.Commands;


var app = new CommandApp();
app.Configure(
  o =>
  {
    o.AddCommand<ScanCommand>("scan");
    o.AddCommand<InfoCommand>("info");
  });

return await app.RunAsync(args);
