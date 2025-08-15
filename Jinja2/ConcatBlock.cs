using System.Text;

namespace Jinja2;

public class ConcatBlock(List<ExpressionBlock?> expressionBlocks) : ExpressionBlock, IRender, IPerformFunction
{
    private StringBlock? _cache;
    
    public override object GetValue(Context context)
    {
        if (_cache is not null)
            return _cache.GetValue(context);

        _cache = BuildCache(context);
        return _cache.GetValue(context);
    }

    public void Render(Context context) => RenderableBlock.Render(context, this, GetValue(context).ToString() ?? string.Empty);

    public void Perform(string name, Context context)
    {
        _cache ??= BuildCache(context);
        _cache.Perform(name, context);
    }

    private StringBlock BuildCache(Context context)
    {
        var builder = new StringBuilder();
        foreach (var expressionBlock in expressionBlocks.OfType<ExpressionBlock>())
        {
            builder.Append(expressionBlock.GetValue(context));
        }

        // We no longer need these because we have the result
        expressionBlocks.Clear();

        return new StringBlock(builder.ToString(), false);
    }
}