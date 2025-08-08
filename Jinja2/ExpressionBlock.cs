namespace Jinja2;

public abstract class ExpressionBlock : Block
{
    public abstract object? GetValue(Context context);
}