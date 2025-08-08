namespace Jinja2;

internal class ExpressionVisitor : JinjaParserBaseVisitor<Block>
{
    public override Block VisitText(JinjaParser.TextContext context)
        => new TextBlock(context.GetText());

    public override Block VisitExpression(JinjaParser.ExpressionContext context) 
        => Visit(context.expression_body());

    public override Block VisitStatement(JinjaParser.StatementContext context) 
        => Visit(context.statement_body());

    public override Block VisitEqAssign(JinjaParser.EqAssignContext context)
    {
        var name = context.ID().GetText();
        var expression = Visit(context.expression_body()) as ExpressionBlock;
        return new SetBlock(name, expression!);
    }

    public override Block VisitEqAdd(JinjaParser.EqAddContext context)
    {
        var left = Visit(context.left) as ExpressionBlock;
        var right = Visit(context.right) as ExpressionBlock;
        
        return new MathBlock(left, right,
            context.@operator.Type == JinjaLexer.PLUS ? Operator.Add : Operator.Subtract);
    }
    
    public override Block VisitEqMul(JinjaParser.EqMulContext context)
    {
        var left = Visit(context.left) as ExpressionBlock;
        var right = Visit(context.right) as ExpressionBlock;
        
        return new MathBlock(left, right,
            context.@operator.Type == JinjaLexer.MUL ? Operator.Mul : Operator.Div);
    }

    public override Block VisitEqINT(JinjaParser.EqINTContext context) 
        => new IntBlock(int.Parse(context.INT().GetText()));
    
    public override Block VisitEqID(JinjaParser.EqIDContext context) 
        => new IdBlock(context.ID().GetText());
}