namespace Jinja2;

public class CollectionBlock(List<ExpressionBlock> items) : ExpressionBlock
{
    public override object? GetValue(Context context) => items;
}