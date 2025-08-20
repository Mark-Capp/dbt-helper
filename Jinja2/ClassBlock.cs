namespace Jinja2;

public class ClassBlock(Dictionary<string, ExpressionBlock> properties) :ExpressionBlock, IHaveProperties
{
    public ExpressionBlock GetProperty(string name) 
        => properties[name];

    public void SetProperty(string name, ExpressionBlock block) 
        => properties[name] = block;


    public override object? GetValue(Context context) => null;
}