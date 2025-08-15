namespace Jinja2;

public class MacroBlock(string name,
    Dictionary<string, ExpressionBlock?> args,
    IBlock[] body) : Block, IRender
{
    public Dictionary<string, ExpressionBlock?> ArgNames { get; } = args;
    
    public void Render(Context context) 
        => context.Macros.TryAdd(name, this);

    public void Execute(Context context)
    {
        foreach (var block in body) {
            if(block is IRender render)
                render.Render(context);
        }
    }
}