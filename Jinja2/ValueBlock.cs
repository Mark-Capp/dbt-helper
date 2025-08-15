namespace Jinja2;

public class ValueBlock(object? value) : ExpressionBlock
{
    public override object? GetValue(Context context) => value;
}