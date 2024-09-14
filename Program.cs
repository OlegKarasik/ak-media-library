using Spectre.Console.Cli;

using MediaLibrary.Commands;

var app = new CommandApp();
app.Configure(
  o =>
  {
    o.AddCommand<OpenDirectoryCommand>("open");
  });

return await app.RunAsync(args);
