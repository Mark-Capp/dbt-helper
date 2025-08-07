namespace Jinja2;

public class Template
{
    private readonly Renderer _renderer;
    private readonly string _content;

    private Template(Renderer renderer, string content)
    {
        _renderer = renderer;
        _content = content;
    }

    public static Template FromString(string content) 
        => new(new Renderer(), content);

    public string Render()
    {
        var blocks = Renderer.Render(_content);
        if (blocks == null)
        {
            return string.Empty;
        }

        var context = new Context();
        foreach (var block in blocks)
            if (block is IRender renderBlock)
                renderBlock.Render(context);

        return context.Content.ToString();
    }
}