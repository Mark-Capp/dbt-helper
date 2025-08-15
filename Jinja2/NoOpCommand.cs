namespace Jinja2;

public class NoOpCommand : ICommand
{
    public static object? Execute() => null;
}