namespace Jinja2;

public class MulCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IdBlock l, { } r) => l.Mul(context, r),
            ({} l, IdBlock r) => r.Mul(context, l),
            (IntBlock l, { } r) => l.Mul(context, r),
            (_, _) => 0
        };
    }
}