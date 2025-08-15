namespace Jinja2;

public class IdBlock(string name) : ExpressionBlock, IRender
{
    public void Render(Context context)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return;
        }
        
        RenderableBlock.Render(context, this, value.GetValue(context)?.ToString() ?? string.Empty);
    }

    public override object? GetValue(Context context) 
        => !context.Variables.TryGetValue(name, out var value) ? null : value.GetValue(context);

    public object? Add(Context context, ExpressionBlock block) 
        => !context.Variables.TryGetValue(name, out var value) ? null : AddCommand.Execute(context, value, block);

    public object? Sub(Context context, ExpressionBlock expressionBlock) 
        => !context.Variables.TryGetValue(name, out var value) ? null : SubCommand.Execute(context,value,expressionBlock);

    public object? Div(Context context, ExpressionBlock expressionBlock) 
        => !context.Variables.TryGetValue(name, out var value) ? null : DivCommand.Execute(context, value, expressionBlock);

    public object? Mul(Context context, ExpressionBlock expressionBlock) 
        => !context.Variables.TryGetValue(name, out var value) ? null : MulCommand.Execute(context, value, expressionBlock);
}