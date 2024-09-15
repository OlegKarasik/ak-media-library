using Spectre.Console.Cli;

using MediaLibrary.Commands;

var app = new CommandApp();
app.Configure(
  o =>
  {
    o.AddCommand<ScanLibraryCommand>("scan");
    o.AddCommand<OpenLibraryCommand>("open");
  });

return await app.RunAsync(args);
