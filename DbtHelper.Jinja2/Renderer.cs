using Antlr4.Runtime;

namespace DbtHelper.Jinja2;

internal class Renderer
{
    public static List<Block>? Render(string content)
    {
        var inputStream = new AntlrInputStream(content); 
        var lexer = new JinjaLexer(inputStream);
        
        var tokens  = new CommonTokenStream(lexer);
        var parser = new JinjaParser(tokens);
      
        var jinjaVisitor = new JinjaVisitor();
        var expression = jinjaVisitor
            .Visit(parser.file());
        
        return expression;
    }
}