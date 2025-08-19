namespace Jinja2;

public class ComparisonBlock(
    ExpressionBlock left,
    ExpressionBlock right,
    Operator @operator) : ExpressionBlock
{
    public override object? GetValue(Context context)
    {
        var l = left?.GetValue(context);
        var r = right?.GetValue(context);

        // Equality/inequality for nulls
        if (@operator is Operator.Equal or Operator.NotEqual)
        {
            var equals = Equals(l, r);
            return @operator == Operator.Equal ? equals : !equals;
        }

        // Attempt integer comparison if both are ints
        if (l is int li && r is int ri)
        {
            return CompareInts(li, ri);
        }

        // Fallback to string comparison (ordinal)
        var ls = l?.ToString() ?? string.Empty;
        var rs = r?.ToString() ?? string.Empty;
        var cmp = string.CompareOrdinal(ls, rs);
        return CompareBySign(cmp);
    }

    private object CompareInts(int li, int ri)
        => @operator switch
        {
            Operator.GreaterThan => li > ri,
            Operator.LessThan => li < ri,
            Operator.GreaterThanOrEqual => li >= ri,
            Operator.LessThanOrEqual => li <= ri,
            Operator.Equal => li == ri,
            Operator.NotEqual => li != ri,
            _ => false
        };

    private object CompareBySign(int cmp)
        => @operator switch
        {
            Operator.GreaterThan => cmp > 0,
            Operator.LessThan => cmp < 0,
            Operator.GreaterThanOrEqual => cmp >= 0,
            Operator.LessThanOrEqual => cmp <= 0,
            Operator.Equal => cmp == 0,
            Operator.NotEqual => cmp != 0,
            _ => false
        };
}
