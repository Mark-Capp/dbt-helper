namespace Jinja2;

public class MathBlock(
    ExpressionBlock left,
    ExpressionBlock right,
    Operator @operator) : ExpressionBlock, IRender
{
    public override object? GetValue(Context context) =>
        @operator switch
        {
            Operator.Add => AddCommand.Execute(context, left, right),
            Operator.Subtract => SubCommand.Execute(context, left, right),
            Operator.Mul => MulCommand.Execute(context, left, right),
            Operator.Div => DivCommand.Execute(context, left, right),
            _ => NoOpCommand.Execute() 
        };

    public void Render(Context context) 
        => RenderableBlock.Render(context, this, GetValue(context)?.ToString() ?? string.Empty);
}