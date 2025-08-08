namespace Jinja2;

public class MacroBlock(string name,
    string[] argNames,
    Block[] body) : Block, IRender
{
    public string[] ArgNames { get; } = argNames;
    
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