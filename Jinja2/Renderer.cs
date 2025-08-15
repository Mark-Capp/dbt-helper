using Antlr4.Runtime;

namespace Jinja2;

internal class Renderer
{
    public static List<IBlock>? Render(string content)
    {
        var inputStream = new AntlrInputStream(content); 
        var lexer = new JinjaLexer(inputStream);
        
        var tokens  = new CommonTokenStream(lexer);
        var parser = new JinjaParser(tokens);
      
        var jinjaVisitor = new JinjaVisitor();
        var expression = jinjaVisitor
            .Visit(parser.template());
        
        return expression;
    }
}