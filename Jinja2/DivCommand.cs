namespace Jinja2;

public class DivCommand : ICommand
{
    public static object? Execute(Context context, ExpressionBlock left, ExpressionBlock right)
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