namespace Jinja2;

public class AddCommand : ICommand
{
    public static object? Execute(Context context, ExpressionBlock left, ExpressionBlock right)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IdBlock l, { } r) => l.Add(context, r),
            ({} l, IdBlock r) => r.Add(context, l),
            (IntBlock l, { } r) => l.Add(context, r),
            (StringBlock l, {} r ) => l.Add(context, r),
            (_, _) => 0
        };
    }
}