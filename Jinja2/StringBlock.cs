namespace Jinja2;

public class StringBlock : ExpressionBlock, IRender, IPerformFunction
{
    private string _value;
    private readonly Dictionary<string, Action<Context>> _functions;
    
    public StringBlock(string value)
    {
        _value = value[1..^1];
        _functions = new Dictionary<string, Action<Context>>
        {
            { "upper", ToUpper }
        };
    }

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

    private void ToUpper(Context context) => _value = _value.ToUpper();
    
    public void Perform(string name, Context context)
    {
        if (_functions.TryGetValue(name, out var action))
        {
            action(context);
        }
    }
}