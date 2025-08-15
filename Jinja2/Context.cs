using System.Text;

namespace Jinja2;

public class Context
{
    public StringBuilder Builder { get; } = new();
    public Dictionary<string, ExpressionBlock> Variables { get; set; } = new();
    public Dictionary<string, MacroBlock> Macros { get; set; } = new();
}