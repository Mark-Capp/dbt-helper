using System.Text;

namespace Jinja2;

public class ConcatBlock(List<ExpressionBlock?> expressionBlocks) : ExpressionBlock, IRender, IPerformFunction
{
    public override object GetValue(Context context)
    {
        var builder = new StringBuilder();
        foreach (var expressionBlock in expressionBlocks.OfType<ExpressionBlock>())
        {
            builder.Append(expressionBlock.GetValue(context));
        }

        return builder.ToString();
    }

    public void Render(Context context) 
        => context.Content += GetValue(context);

    public void Perform(string name, Context context)
    {
        foreach (var expressionBlock in expressionBlocks.OfType<IPerformFunction>())
        {
            expressionBlock.Perform(name, context);
        }
    }
}