namespace Jinja2;

public interface IRender
{
    void Render(Context context);
}

public static class RenderableBlock 
{
    public static void Render(Context context, Block block, string content)
    {
        if (string.IsNullOrWhiteSpace(content) && block.ShouldStripNewLines)
        {
            return;
        }
        
        context.Content += content;
    }
}

public interface IPerformFunction
{
    void Perform(string name, Context context);
    
}