namespace Jinja2;

public interface ICommand
{
    object? Execute(Context context);
}