namespace Jinja2;

public class PropertyBlock(string name, string[] properties) : ExpressionBlock, IRender
{
    public void Render(Context context)
    {
        var thisValue = GetValue(context);
        if (thisValue == null)
        {
            return;
        }
        
        RenderableBlock.Render(context, this, thisValue?.ToString() ?? string.Empty);
    }

    public override object? GetValue(Context context)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return null;
        }

        if (value is IHaveProperties haveProperties)
        {
            var propertyValue = haveProperties.GetProperty(properties[0]);
            return propertyValue.GetValue(context);
        }

        return null;
    }
}

public interface IHaveProperties
{
    public ExpressionBlock GetProperty(string name);
}