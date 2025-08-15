namespace Jinja2;

public class IdBlock(string name) : ExpressionBlock, IRender
{
    public void Render(Context context)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return;
        }
        context.Content += value.GetValue(context);
    }

    public override object? GetValue(Context context) 
        => !context.Variables.TryGetValue(name, out var value) ? null : value.GetValue(context);

    public object? Add(Context context, ExpressionBlock block)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return null;
        }
        
        var command = new AddCommand(value, block);
        return command.Execute(context);
    }

    public object? Sub(Context context, ExpressionBlock expressionBlock)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return null;
        }
        
        var command = new SubCommand(value, expressionBlock);
        return command.Execute(context);
    }

    public object? Div(Context context, ExpressionBlock expressionBlock)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return null;
        }

        var command = new DivCommand(value, expressionBlock);
        return command.Execute(context);
    }
    
    public object? Mul(Context context, ExpressionBlock expressionBlock)
    {
        if (!context.Variables.TryGetValue(name, out var value))
        {
            return null;
        }

        var command = new MulCommand(value, expressionBlock);
        return command.Execute(context);
    }
}