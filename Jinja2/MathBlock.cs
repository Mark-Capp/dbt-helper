namespace Jinja2;

public class MathBlock(
    ExpressionBlock left,
    ExpressionBlock right,
    Operator @operator) : ExpressionBlock, IRender
{
    public override object? GetValue(Context context)
    {
        ICommand command = @operator switch
        {
            Operator.Add => new AddCommand(left, right),
            Operator.Subtract => new SubCommand(left, right),
            Operator.Mul => new MulCommand(left, right),
            Operator.Div => new DivCommand(left, right),
            _ => new NoOpCommand() 
        };

        return command.Execute(context);
    }

    public void Render(Context context) 
        => RenderableBlock.Render(context, this, GetValue(context)?.ToString() ?? string.Empty);
}