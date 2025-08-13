namespace Jinja2;

public class AddCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock l, {} rightInt) => l.Add(context, rightInt),
            (IdBlock l, { } r) => l.Add(context, r),
            (StringBlock l, {} r ) => l.Add(context, r),
            (_, _) => 0
        };
    }
}