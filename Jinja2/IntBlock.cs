using System.Numerics;

namespace Jinja2;

public class IntBlock(int value) : ExpressionBlock, IAdditionOperators<IntBlock, IntBlock, int>
{
    public override object GetValue(Context context) => value;

    public object Add(Context context, ExpressionBlock right) =>
        right switch
        {
            IntBlock intBlock => this + intBlock,
            IdBlock idBlock => idBlock.Add(context, this),
        };

    public object Sub(Context context, ExpressionBlock right) =>
        right switch
        {
            IntBlock intBlock => this - intBlock,
            IdBlock idBlock => idBlock.Sub(context, this),
        };

    public object Mul(Context context, ExpressionBlock right) =>
        right switch
        {
            IntBlock intBlock => this - intBlock,
            IdBlock idBlock => idBlock.Mul(context, this),
        };

    public object Div(Context context, ExpressionBlock right) =>
        right switch
        {
            IntBlock intBlock => this - intBlock,
            IdBlock idBlock => idBlock.Div(context, this),
        };

    public static int operator +(IntBlock left, IntBlock right)
    {
        var context = new Context();
        var l = left.GetValue(context) as int? ?? 0;
        var r = right.GetValue(context) as int? ?? 0;
        return l + r;
    }
    
    public static int operator -(IntBlock left, IntBlock right)
    {
        var context = new Context();
        var l = left.GetValue(context) as int? ?? 0;
        var r = right.GetValue(context) as int? ?? 0;
        return l - r;
    }
    
    public static int operator *(IntBlock left, IntBlock right)
    {
        var context = new Context();
        var l = left.GetValue(context) as int? ?? 0;
        var r = right.GetValue(context) as int? ?? 0;
        return l * r;
    }
    
    public static int operator /(IntBlock left, IntBlock right)
    {
        var context = new Context();
        var l = left.GetValue(context) as int? ?? 0;
        var r = right.GetValue(context) as int? ?? 0;
        return l / r;
    }
}