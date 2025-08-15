namespace Jinja2;

public class Template
{
    private readonly Renderer _renderer;
    private readonly string[] _content;

    private Template(Renderer renderer, string[] content)
    {
        _renderer = renderer;
        _content = content;
    }

    public static Template FromString(string content) 
        => new(new Renderer(), [content]);
    
    public static Template FromString(string[] content) 
        => new(new Renderer(), content);

    public string Render() => Render(new Context());

    public string Render(Context context) 
        => CreateContext(context).Content;

    public Context CreateContext(Context? context = null)
    {
        context ??= new Context();
        foreach (var content in _content)
        {
            var blocks = Renderer.Render(content);
            if (blocks == null)
            {
                continue;
            }
            
            foreach (var block in blocks)
                if (block is IRender renderBlock)
                    renderBlock.Render(context);
        }

        return context;
    }
}