namespace Jinja2;

public interface IRender
{
    void Render(Context context);
}

public interface IPerformFunction
{
    void Perform(string name, Context context);
    
}