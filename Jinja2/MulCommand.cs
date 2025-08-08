namespace Jinja2;

public class MulCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock leftInt, IntBlock rightInt) => leftInt.Mul(context, rightInt),
            (_, _) => 0
        };
    }
}