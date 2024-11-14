using Spectre.Console.Cli;

using MediaLibrary.Commands;


var app = new CommandApp();
app.Configure(
  o =>
  {
    o.AddCommand<ScanCommand>("scan");
    o.AddCommand<NormalizeCommand>("open");
  });

return await app.RunAsync(args);
