namespace Jinja2;

public class SetBlock(string name, ExpressionBlock value) : Block, IRender
{
    public void Render(Context context) 
        => context.Variables.TryAdd(name, value);
}