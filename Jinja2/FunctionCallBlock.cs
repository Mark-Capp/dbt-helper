namespace Jinja2;

public class FunctionCallBlock(ExpressionBlock subject, List<string> functions)
    : ExpressionBlock , IRender
{
    public override object? GetValue(Context context)
    {
        if (subject is not IPerformFunction performer) 
            return subject.GetValue(context);
        
        foreach (var function in functions)
        {
            performer.Perform(function, context);
        }
        return subject.GetValue(context);
    }

    public void Render(Context context)
        => RenderableBlock.Render(context, this, GetValue(context)?.ToString() ?? string.Empty);

}