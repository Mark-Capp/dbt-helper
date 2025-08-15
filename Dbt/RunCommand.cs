using System.Reflection;
using Jinja2;

namespace Dbt;

public class RunCommand
{
    public string Execute(string content)
    {
        var files = Files();
        var template = Template.FromString(content);
        return template.Render(files);
    }

    private Context Files()
    {
        var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var files = names.Select(ReadAsync).ToArray();

        var renderer = Template.FromString(files);
        return renderer.CreateContext();
    }

    private static string ReadAsync(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(name);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}