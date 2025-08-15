namespace Jinja2;

internal class ExpressionVisitor : JinjaParserBaseVisitor<IBlock>
{
    public override IBlock VisitText(JinjaParser.TextContext context)
        => new TextBlock(context.GetText());

    public override IBlock VisitExpression(JinjaParser.ExpressionContext context)
    {
        var block = Visit(context.expression_body());
        block.ShouldStripNewLines = context.MINUS() != null;
        return block;
    }

    public override IBlock VisitStatement(JinjaParser.StatementContext context) 
        => Visit(context.statement_body());

    public override IBlock VisitEqParan(JinjaParser.EqParanContext context) 
        => Visit(context.expression_body());

    public override IBlock VisitEqAssign(JinjaParser.EqAssignContext context)
    {
        var name = context.ID().GetText();
        var expression = Visit(context.expression_body()) as ExpressionBlock;
        return new SetBlock(name, expression!);
    }

    public override IBlock VisitEqAdd(JinjaParser.EqAddContext context)
    {
        var left = Visit(context.left) as ExpressionBlock;
        var right = Visit(context.right) as ExpressionBlock;
        
        return new MathBlock(left, right,
            context.@operator.Type == JinjaLexer.PLUS ? Operator.Add : Operator.Subtract);
    }
    
    public override IBlock VisitEqMul(JinjaParser.EqMulContext context)
    {
        var left = Visit(context.left) as ExpressionBlock;
        var right = Visit(context.right) as ExpressionBlock;
        
        return new MathBlock(left, right,
            context.@operator.Type == JinjaLexer.MUL ? Operator.Mul : Operator.Div);
    }

    public override IBlock VisitEqINT(JinjaParser.EqINTContext context) 
        => new IntBlock(int.Parse(context.INT().GetText()));
    
    public override IBlock VisitEqID(JinjaParser.EqIDContext context) 
        => new IdBlock(context.ID().GetText());
    
    public override IBlock VisitEqString(JinjaParser.EqStringContext context) 
        => new StringBlock(context.STRING().GetText());

    public override IBlock VisitEqMacro(JinjaParser.EqMacroContext context)
    {
        var name = context.ID();
        var args = context.@params().param();
        
        var dictionary = new Dictionary<string, ExpressionBlock?>();
        foreach (var param in args)
        {
            var parameter = param.ID();
            if (param.expression_body() != null)
            {
                var defaultValue = Visit(param.expression_body()) as ExpressionBlock;
                dictionary.Add(parameter.GetText(), defaultValue);
            }
            else
            {
                dictionary.Add(parameter.GetText(), null);
            }
        }
        
        var jinjaVisitor = new JinjaVisitor();
        var body = jinjaVisitor.Visit(context.macro_template());
        foreach (var bodyBlocks in body)
        {
            bodyBlocks.ShouldStripNewLines = context.MINUS() != null;
        }
        
        var block =  new MacroBlock(name.GetText(),
            dictionary,
            body.ToArray())
        {
            ShouldStripNewLines = context.MINUS() != null
        };
        return block;
    }

    public override IBlock VisitEqConcatFuntion(JinjaParser.EqConcatFuntionContext context)
    {
        var concat = context.concat()
            .Select(block => Visit(block) as ExpressionBlock)
            .Select(expression => expression!)
            .ToList();

        var concatBlock = new ConcatBlock(concat);
        var function = context.functionCall().ID().GetText();
        return new FunctionCallBlock(concatBlock, [function]);
    }

    public override IBlock VisitFunctionCall(JinjaParser.FunctionCallContext context)
    {
        var name = context.ID().GetText();
        var args = context.argList()?
            .expression_body()
            .Select(block => Visit(block) as ExpressionBlock)
            .Select(expression => expression!).ToList();
        return new MacroCallBlock(name, args ?? []);
    }

    public override IBlock VisitEqInnerConcat(JinjaParser.EqInnerConcatContext context) => Visit(context.concat());

    public override IBlock VisitEqOuterConcat(JinjaParser.EqOuterConcatContext context)
    {
        var expressionBlocks = context
            .concat_expression_body()
            .Select(value => Visit(value) as ExpressionBlock)
            .ToList();

        return new ConcatBlock(expressionBlocks);
    }

    public override IBlock VisitConcat_expression_body(JinjaParser.Concat_expression_bodyContext context)
    {
        if (context.ID() != null)
            return new IdBlock(context.ID().GetText());
        if(context.STRING() != null)
            return new StringBlock(context.STRING().GetText());
        if(context.INT() != null)
            return new IntBlock(int.Parse(context.INT().GetText()));

        return null;
    }
    
}