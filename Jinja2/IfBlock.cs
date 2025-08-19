namespace Jinja2;

public class IfBlock(
    ExpressionBlock condition,
    IBlock[] ifBody,
    List<(ExpressionBlock condition, IBlock[] body)> elifs,
    IBlock[]? elseBody) : Block, IRender
{
    public void Render(Context context)
    {
        if (IsTrue(condition.GetValue(context)))
        {
            RenderBody(context, ifBody);
            return;
        }

        foreach (var (elifCond, body) in elifs)
        {
            if (IsTrue(elifCond.GetValue(context)))
            {
                RenderBody(context, body);
                return;
            }
        }

        if (elseBody is not null)
        {
            RenderBody(context, elseBody);
        }
    }

    private void RenderBody(Context context, IEnumerable<IBlock> body)
    {
        foreach (var block in body)
        {
            if (block is IRender render)
                render.Render(context);
        }
    }

    private static bool IsTrue(object? value)
        => value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            string s => !string.IsNullOrEmpty(s),
            _ => true // default truthy for other objects
        };
}
