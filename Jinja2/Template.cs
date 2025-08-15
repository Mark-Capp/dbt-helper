namespace Jinja2;

public class Template(string[] templateContent)
{
    public static Template FromString(string content) 
        => new([content]);
    
    public static Template FromString(string[] content) 
        => new(content);

    public string Render() => Render(new Context());

    public string Render(Context context) 
        => CreateContext(context).Builder.ToString();

    public Context CreateContext(Context? context = null)
    {
        context ??= new Context();
        foreach (var content in templateContent)
        {
            var blocks = Renderer.Render(content);
            if (blocks == null)
                continue;
            
            foreach (var block in blocks)
                if (block is IRender renderBlock)
                    renderBlock.Render(context);
        }

        return context;
    }
}