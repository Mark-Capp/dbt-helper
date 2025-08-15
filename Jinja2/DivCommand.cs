namespace Jinja2;

public class DivCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IdBlock l, { } r) => l.Div(context, r),
            (IntBlock l, { } r) => l.Div(context, r),
            (_, _) => 0
        };
    }
}