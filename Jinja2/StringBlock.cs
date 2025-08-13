namespace Jinja2;

public class StringBlock(string value) : ExpressionBlock, IRender
{
    public override object GetValue(Context context) => value[1..^1];

    public void Render(Context context)
    {
        var toRender = value[1..^1];
        context.Content += toRender;
    }
}