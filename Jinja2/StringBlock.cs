namespace Jinja2;

public class StringBlock(string value) : ExpressionBlock, IRender
{
    private readonly string _value = value[1..^1];

    public override object GetValue(Context context) => _value;

    public void Render(Context context) => context.Content += _value;

    public object Add(Context context, ExpressionBlock right)
    {
        return right switch
        {
            IntBlock intBlock => $"{_value}{intBlock.GetValue(context)}",
            IdBlock idBlock => $"{_value}{idBlock.GetValue(context)}",
            StringBlock stringBlock => $"{GetValue(context)}{stringBlock.GetValue(context)}"
        };
    }
}