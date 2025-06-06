using System.Reflection;

namespace DbtHelper.Jinja.Tests;

public class Helper
{
    public static string Read(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"DbtHelper.Jinja.Tests.Templates.{fileName}.txt";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    public static string ReadRendered(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"DbtHelper.Jinja.Tests.Templates.{fileName}.txt.rendered";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}