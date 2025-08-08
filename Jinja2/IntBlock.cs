namespace Jinja2;

public class IntBlock(int value) : ExpressionBlock
{
    public override object GetValue(Context context) => value;

    public int Add(Context context, IntBlock block)
    {
        var right = block.GetValue(context) as int? ?? 0;
        return value + right;
    }
    
    public int Mul(Context context, IntBlock block)
    {
        var right = block.GetValue(context) as int? ?? 0;
        return value * right;
    }
    
    public int Sub(Context context, IntBlock block)
    {
        var right = block.GetValue(context) as int? ?? 0;
        return value - right;
    }
    
    public int Div(Context context, IntBlock block)
    {
        var right = block.GetValue(context) as int? ?? 0;
        return value / right;
    }
}