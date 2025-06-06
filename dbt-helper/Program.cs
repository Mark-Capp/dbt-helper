using System.ComponentModel;
using DbtHelper.Jinja;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var registrations = new ServiceCollection();

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrar = new MyTypeRegistrar(registrations);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<ParseCommand>("parse");
    config.AddCommand<ParseCommand>("parse2");
});
await app.RunAsync(args);

public class ParseCommand : AsyncCommand<ParseCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Path to search. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? SearchPath { get; init; }
        
        [Description("File to Parse")]
        [CommandOption("-f|--file")]
        public string? File { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var searchPath = settings.SearchPath ?? Directory.GetCurrentDirectory();
        var file = settings.File ?? "";
        
        var fileOnDisk = new DirectoryInfo(searchPath)
            .GetFiles(file)
            .FirstOrDefault();
        
        if (fileOnDisk == null)
        {
            AnsiConsole.MarkupLine($"No file found under '{searchPath}'");
            return 1;
        }

        AnsiConsole.MarkupLine($"Reading file");
        using var sr = new StreamReader(fileOnDisk.OpenRead());
        var fileContent = await sr.ReadToEndAsync();

        AnsiConsole.MarkupLine($"Rendering template");
        var template = Template.FromString(fileContent);
        var result = template.Render(new {});

        AnsiConsole.MarkupLine($"Writing new file");
        var newFile = Path.Combine(searchPath, $"{fileOnDisk.Name}.rendered");
        await File.WriteAllTextAsync(newFile, result);
        
        return 0;
    }


    string Ref(string content)
    {
        return content;
    }
}

public class MyTypeRegistrar(ServiceCollection registrations) : ITypeRegistrar
{
    public void Register(Type service, Type implementation) => registrations.AddScoped(service, implementation);

    public void RegisterInstance(Type service, object implementation) => registrations.AddScoped(service, _ => implementation);

    public void RegisterLazy(Type service, Func<object> factory) => registrations.AddScoped(service, _ => factory);

    public ITypeResolver Build() => new TypeResolver(registrations.BuildServiceProvider());
}

public class TypeResolver(ServiceProvider buildServiceProvider) : ITypeResolver
{
    public object? Resolve(Type? type) => buildServiceProvider.GetService(type);
}