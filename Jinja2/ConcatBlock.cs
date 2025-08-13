using System.Text;

namespace Jinja2;

public class ConcatBlock(List<ExpressionBlock?> expressionBlocks) : ExpressionBlock, IRender
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
}