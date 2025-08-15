namespace Jinja2;

public class SubCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public static object? Execute(Context context, ExpressionBlock left, ExpressionBlock right)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock leftInt, IntBlock rightInt) => leftInt.Sub(context, rightInt),
            (IdBlock leftInt, { } r) => leftInt.Sub(context, r),
            (_, _) => 0
        };
    }
    
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock leftInt, IntBlock rightInt) => leftInt.Sub(context, rightInt),
            (IdBlock leftInt, { } r) => leftInt.Sub(context, r),
            (_, _) => 0
        };
    }
}