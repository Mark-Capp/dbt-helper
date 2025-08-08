namespace Jinja2;

public class NoOpCommand : ICommand
{
    public object? Execute(Context context) => null;
}