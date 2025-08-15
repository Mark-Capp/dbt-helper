namespace Jinja2;

public class ValueBlock(object? value) : ExpressionBlock, IRender
{
    public override object? GetValue(Context context) => value;

    public void Render(Context context) 
        => RenderableBlock.Render(context, this, value?.ToString() ?? string.Empty);
}