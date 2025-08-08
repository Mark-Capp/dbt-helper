namespace Jinja2;

public class TextBlock(string content) 
    : Block, IRender
{
    public void Render(Context context) 
        => context.Content += content;
}