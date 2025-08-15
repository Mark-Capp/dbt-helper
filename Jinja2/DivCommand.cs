namespace Jinja2;

public class DivCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
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