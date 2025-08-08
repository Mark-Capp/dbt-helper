namespace Jinja2;

public class AddCommand(ExpressionBlock left, ExpressionBlock right) : ICommand
{
    public object? Execute(Context context)
    {
        var tuple = (left, right);
        return tuple switch
        {
            (IntBlock leftInt, IntBlock rightInt) => leftInt.Add(context, rightInt),
            (IdBlock leftInt, { } r) => leftInt.Add(context, r),
            ({} l, IdBlock r) => r.Add(context, l),
            (_, _) => 0
        };
    }
}