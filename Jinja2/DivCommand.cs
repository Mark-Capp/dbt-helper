namespace Jinja2;

public class DivCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock leftInt, IntBlock rightInt) => leftInt.Div(context, rightInt),
            (_, _) => 0
        };
    }
}