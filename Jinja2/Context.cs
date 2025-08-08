namespace Jinja2;

public class Context
{
    public string Content { get; set; } = ""; 
    public Dictionary<string, ExpressionBlock> Variables { get; set; } = new();
}