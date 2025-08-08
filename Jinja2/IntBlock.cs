using System.Numerics;

namespace Jinja2;

public class IntBlock(int value) : ExpressionBlock, IAdditionOperators<IntBlock, IntBlock, int>
{
    public override object GetValue(Context context) => value;

    public object Add(Context context, ExpressionBlock right)
    {
        return right switch
        {
            IntBlock intBlock => this + intBlock,
            IdBlock idBlock => idBlock.Add(context, this),
        };
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

    public static int operator +(IntBlock left, IntBlock right)
    {
        var context = new Context();
        var l = left.GetValue(context) as int? ?? 0;
        var r = right.GetValue(context) as int? ?? 0;
        return l + r;
    }
}