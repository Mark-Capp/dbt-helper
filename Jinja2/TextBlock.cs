namespace Jinja2;

public class TextBlock(string content) 
    : Block, IRender
{
    public void Render(Context context)
    {
        
        RenderableBlock.Render(context, this, content);

    }
}