using System.Collections;

namespace DbtHelper.Jinja
{
    internal abstract class SyntaxNode
    {
        internal int Start { get; }

        internal int End { get; }

        internal protected SyntaxNode(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    #region Block
    internal abstract class Block : SyntaxNode
    {
        protected internal Block(int start, int end)
            : base(start, end) { }

        public abstract void Render(Renderer.Context ctx);
        
        protected ExpressionValue Evaluate(Renderer.Context ctx, Expression expression)
        {
            return expression switch
            {
                UnaryExpression e => EvaluateUnary(ctx, e),
                BinaryExpression e => EvaluateBinary(ctx, e),
                ParenthesisExpression e => EvaluateParenthesis(ctx, e),
                LiteralExpression e => EvaluateLiteral(ctx, e),
                SymbolExpression e => EvaluateSymbol(ctx, e),
                _ => throw new InvalidOperationException("Unknown expression to evaluate")
            };
        }

        private ExpressionValue EvaluateUnary(Renderer.Context ctx, UnaryExpression e)
        {
            return e.Operator switch
            {
                UnaryOperator.Negative => EvaluateNegative(ctx, e.Expression),
                UnaryOperator.Positive => EvaluatePositive(ctx, e.Expression),
                UnaryOperator.Not => EvaluateNot(ctx, e.Expression),
                _ => throw new InvalidOperationException($"Unknown unary operator: {e.Operator}")
            };
        }

        private ExpressionValue EvaluateBinary(Renderer.Context ctx, BinaryExpression e)
        {
            return e.Operator switch
            {
                BinaryOperator.Pipe => EvaluatePipe(ctx, e.Left, e.Right),
                BinaryOperator.Subscript => EvaluateSubscript(ctx, e.Left, e.Right),
                BinaryOperator.MemberAccess => EvaluateMemberAccess(ctx, e.Left, e.Right),
                BinaryOperator.Or => EvaluateOr(ctx, e.Left, e.Right),
                BinaryOperator.And => EvaluateAnd(ctx, e.Left, e.Right),
                BinaryOperator.Add => EvaluateAdd(ctx, e.Left, e.Right),
                BinaryOperator.Substract => EvaluateSubstract(ctx, e.Left, e.Right),
                BinaryOperator.Multiply => EvaluateMultiply(ctx, e.Left, e.Right),
                BinaryOperator.DivideFloat => EvaluateDivideFloat(ctx, e.Left, e.Right),
                BinaryOperator.DivideInteger => EvaluateDivideInteger(ctx, e.Left, e.Right),
                BinaryOperator.Modulo => EvaluateModulo(ctx, e.Left, e.Right),
                BinaryOperator.Less => EvaluateLess(ctx, e.Left, e.Right),
                BinaryOperator.LessOrEqual => EvaluateLessOrEqual(ctx, e.Left, e.Right),
                BinaryOperator.Equal => EvaluateEqual(ctx, e.Left, e.Right),
                BinaryOperator.GreaterOrEqual => EvaluateGreaterOrEqual(ctx, e.Left, e.Right),
                BinaryOperator.Greater => EvaluateGreater(ctx, e.Left, e.Right),
                BinaryOperator.NotEqual => EvaluateNotEqual(ctx, e.Left, e.Right),
                _ => throw new InvalidOperationException($"Unknown binary operator: {e.Operator}")
            };
        }

        private ExpressionValue EvaluateParenthesis(Renderer.Context ctx, ParenthesisExpression e) => Evaluate(ctx, e.Expression);

        private static ExpressionValue EvaluateSymbol(Renderer.Context ctx, SymbolExpression e)
        {
            if (ctx.Locals.TryGetValue(e.Symbol, out var value))
                return AsValue(value);
            
            return ContainsValue(ctx.Globals, e.Symbol) 
                ? GetValue(ctx.Globals, e.Symbol) 
                : ExpressionValue.Empty;
        }

        private ExpressionValue EvaluateLiteral(Renderer.Context ctx, LiteralExpression e)
        {
            return e.ValueType switch
            {
                ValueType.String => new ExpressionValue(e.StringValue),
                ValueType.Integer => new ExpressionValue(e.IntegerValue),
                ValueType.Float => new ExpressionValue(e.FloatValue),
                ValueType.Boolean => new ExpressionValue(e.BooleanValue),
                _ => throw new InvalidOperationException("Unknown literal type to evaluate")
            };
        }

        private ExpressionValue EvaluateNegative(Renderer.Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            return v.ValueType switch
            {
                ValueType.Integer => new ExpressionValue(-v.IntegerValue),
                ValueType.Float => new ExpressionValue(-v.FloatValue),
                _ => throw ThrowHelper(e, RenderingError.UnsupportedOperation, ctx)
            };
        }

        private ExpressionValue EvaluatePositive(Renderer.Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            return v.ValueType switch
            {
                ValueType.Integer or ValueType.Float => v,
                _ => throw ThrowHelper(e, RenderingError.UnsupportedOperation, ctx)
            };
        }

        private ExpressionValue EvaluateNot(Renderer.Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            return new ExpressionValue(!v.BooleanValue);
        }

        private ExpressionValue EvaluatePipe(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var argList = new List<object>();
            argList.Add(vl.ObjectValue);
            string? filterSymbol = null;

            switch (r)
            {
                case SymbolExpression e:
                    filterSymbol = e.Symbol;
                    break;

                case BinaryExpression { Operator: BinaryOperator.FunctionCall, Left: SymbolExpression, Right: ListExpression } e:
                    filterSymbol = ((SymbolExpression)e.Left).Symbol;
                    var args = ((ListExpression)e.Right).Expressions
                        .Select(a => Evaluate(ctx, a).ObjectValue);
                    argList.AddRange(args);
                    break;

                default:
                    throw new InvalidOperationException("Invalid filter syntax");
            }

            if (!ctx.Filters.TryGetValue(filterSymbol, out var value))
                throw ThrowHelper(r, RenderingError.UnsupportedFilter, ctx);

            try
            {
                var result = value.Invoke(null, argList.ToArray());
                return AsValue(result);
            }
            catch (Exception ex)
            {
                throw ThrowHelper(r, RenderingError.FilterCallFailed, ctx, ex);
            }
        }

        private ExpressionValue EvaluateSubscript(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            switch (vl.ObjectValue)
            {
                case IList list:
                    if (vr.IntegerValue >= 0 && vr.IntegerValue < list.Count)
                        return AsValue(list[(int)vr.IntegerValue]);
                    else
                        return ExpressionValue.Empty;

                case IDictionary dict:
                    if (dict.Contains(vr.ObjectValue))
                        return AsValue(dict[vr.ObjectValue]);
                    else
                        return ExpressionValue.Empty;

                default:
                    return ExpressionValue.Empty;
            }
        }

        private ExpressionValue EvaluateMemberAccess(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);

            switch (r)
            {
                case SymbolExpression e:
                    return GetValue(vl.ObjectValue, e.Symbol);

                default:
                    throw new InvalidOperationException("Member is not a symbol");
            }
        }

        private ExpressionValue EvaluateOr(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            if (vl.BooleanValue)
                return new ExpressionValue(true);

            var vr = Evaluate(ctx, r);
            return new ExpressionValue(vr.BooleanValue);
        }

        private ExpressionValue EvaluateAnd(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            if (!vl.BooleanValue)
                return new ExpressionValue(false);

            var vr = Evaluate(ctx, r);
            return new ExpressionValue(vr.BooleanValue);
        }

        private ExpressionValue EvaluateAdd(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    // string + <any> => string
                    return new ExpressionValue(vl.StringValue + vr.StringValue);

                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // int + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        case ValueType.Integer:
                            // int + int => int
                            return new ExpressionValue(vl.IntegerValue + vr.IntegerValue);
                        case ValueType.Float:
                            // int + float => float
                            return new ExpressionValue(vl.FloatValue + vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // float + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float + int/float => float
                            return new ExpressionValue(vl.FloatValue + vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // bool + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        default:
                            throw error();
                    }

                case ValueType.Object:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // obj + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateSubstract(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                            // int - int => int
                            return new ExpressionValue(vl.IntegerValue - vr.IntegerValue);
                        case ValueType.Float:
                            // int - float -> float
                            return new ExpressionValue(vl.FloatValue - vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float - int/float => float
                            return new ExpressionValue(vl.FloatValue - vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateMultiply(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                            // int * int => int
                            return new ExpressionValue(vl.IntegerValue * vr.IntegerValue);
                        case ValueType.Float:
                            // int * float -> float
                            return new ExpressionValue(vl.FloatValue * vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float * int/float => float
                            return new ExpressionValue(vl.FloatValue * vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateDivideFloat(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if ((vl.ValueType == ValueType.Integer || vl.ValueType == ValueType.Float)
                && (vr.ValueType == ValueType.Integer || vr.ValueType == ValueType.Float))
            {
                if (vr.FloatValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero, ctx);
                else
                    return new ExpressionValue(vl.FloatValue / vr.FloatValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);
            }
        }

        private ExpressionValue EvaluateDivideInteger(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if (vl.ValueType == ValueType.Integer && vr.ValueType == ValueType.Integer)
            {
                if (vr.IntegerValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero, ctx);
                else
                    return new ExpressionValue(vl.IntegerValue / vr.IntegerValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);
            }
        }

        private ExpressionValue EvaluateModulo(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if (vl.ValueType == ValueType.Integer && vr.ValueType == ValueType.Integer)
            {
                if (vr.IntegerValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero, ctx);
                else
                    return new ExpressionValue(vl.IntegerValue % vr.IntegerValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);
            }
        }

        private ExpressionValue EvaluateLess(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) < 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue < vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateLessOrEqual(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception Error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) <= 0);
                        default:
                            throw Error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue <= vr.FloatValue);
                        default:
                            throw Error();
                    }

                default:
                    throw Error();
            }
        }

        private ExpressionValue EvaluateEqual(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception Error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) == 0);
                        default:
                            throw Error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue == vr.FloatValue);
                        default:
                            throw Error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.Boolean:
                            return new ExpressionValue(vl.BooleanValue == vr.BooleanValue);
                        default:
                            throw Error();
                    }

                case ValueType.Object:
                    return new ExpressionValue(vl.ObjectValue == vr.ObjectValue);

                default:
                    throw Error();
            }
        }

        private ExpressionValue EvaluateGreaterOrEqual(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception Error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) >= 0);
                        default:
                            throw Error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue >= vr.FloatValue);
                        default:
                            throw Error();
                    }

                default:
                    throw Error();
            }
        }

        private ExpressionValue EvaluateGreater(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            return vl.ValueType switch
            {
                ValueType.String => vr.ValueType switch
                {
                    ValueType.String => new ExpressionValue(string.CompareOrdinal(vl.StringValue, vr.StringValue) > 0),
                    _ => throw Error()
                },
                ValueType.Integer or ValueType.Float => vr.ValueType switch
                {
                    ValueType.Integer or ValueType.Float => new ExpressionValue(vl.FloatValue > vr.FloatValue),
                    _ => throw Error()
                },
                _ => throw Error()
            };

            Exception Error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);
        }

        private ExpressionValue EvaluateNotEqual(Renderer.Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception Error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation, ctx);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) != 0);
                        default:
                            throw Error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue != vr.FloatValue);
                        default:
                            throw Error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.Boolean:
                            return new ExpressionValue(vl.BooleanValue != vr.BooleanValue);
                        default:
                            throw Error();
                    }

                case ValueType.Object:
                    return new ExpressionValue(vl.ObjectValue != vr.ObjectValue);

                default:
                    throw Error();
            }
        }

        private static ExpressionValue AsValue(object? data)
        {
            if (data == null)
            {
                return new ExpressionValue(data);
            }

            return data switch
            {
                string s => new ExpressionValue(s),
                int i => new ExpressionValue(i),
                long l => new ExpressionValue(l),
                float f => new ExpressionValue(f),
                double d => new ExpressionValue(d),
                decimal d => new ExpressionValue(d),
                bool b => new ExpressionValue(b),
                _ => new ExpressionValue(data),
            };
        }

        private static bool ContainsValue(object? data, string? symbol)
        {
            if (data == null)
                return false;

            var t = data.GetType();
            return t.GetProperty(symbol) != null
                || t.GetField(symbol) != null;
        }

        private static ExpressionValue GetValue(object? data, string? symbol)
        {
            if (data == null)
                return ExpressionValue.Empty;
            
            var t = data.GetType();

            var p = t.GetProperty(symbol);
            if (p != null)
                return AsValue(p.GetValue(data));

            var f = t.GetField(symbol);
            return f != null ? AsValue(f.GetValue(data)) : ExpressionValue.Empty;
        }
        
        private static RenderingException ThrowHelper(SyntaxNode n, RenderingError e,
             Renderer.Context context, Exception? innerException = null)
        {
            var (r, c) = TextUtils.GetCharPosition(context.Template, n.Start);
            return new RenderingException(n.Start, r, c, e, innerException);
        }
    }

    internal class TextBlock : Block
    {
        internal TextBlock(int start, int end,
            string content)
            : base(start, end)
        {
            Content = content;
        }

        internal string Content { get; }
        
        public override void Render(Renderer.Context ctx) 
            => ctx.Writer.Write(Content);
    }

    internal class IfStatementBlock : Block
    {
        internal IfStatementBlock(int start, int end,
            IEnumerable<TestBlock> testList)
            : base(start, end)
        {
            TestList = testList;
        }

        internal IEnumerable<TestBlock> TestList { get; }
        
        public override void Render(Renderer.Context ctx)
        {
            foreach (var t in TestList)
            {
                if (t.Test == null)
                {
                    t.Render(ctx);
                }
                else
                {
                    var v = Evaluate(ctx, t.Test);
                    if (!v.BooleanValue) continue;

                    t.Render(ctx);
                    break;
                }

            }

        }
    }

    internal class TestBlock : Block
    {
        internal TestBlock(int start, int end,
            Expression test, IEnumerable<Block> body)
            : base(start, end)
        {
            Test = test;
            Body = body;
        }

        internal Expression Test { get; }

        internal IEnumerable<Block> Body { get; }
        public override void Render(Renderer.Context ctx)
        {
            foreach (var b in Body)
                b.Render(ctx);
        }
    }

    internal class ForStatementBlock : Block
    {
        internal ForStatementBlock(int start, int end,
            Expression loopVariable, Expression iterator, IEnumerable<Block> body)
            : base(start, end)
        {
            LoopVariable = loopVariable;
            Iterator = iterator;
            Body = body;
        }

        internal Expression LoopVariable { get; }

        internal Expression Iterator { get; }

        internal IEnumerable<Block> Body { get; }
        public override void Render(Renderer.Context ctx)
        {
            var loopVariable = (SymbolExpression)LoopVariable;
            var loopVariableName = loopVariable.Symbol;
            var hasShadowedVariable = ctx.Locals.ContainsKey(loopVariableName);
            var shadowedVariable = hasShadowedVariable ?
                ctx.Locals[loopVariableName] : null;

            var iterator = Evaluate(ctx, Iterator);

            if (iterator.ValueType != ValueType.Object
                || !(iterator.ObjectValue is IEnumerable))
                return;

            var enumerator = ((IEnumerable)iterator.ObjectValue).GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (ctx.Locals.ContainsKey(loopVariableName))
                    ctx.Locals[loopVariableName] = enumerator.Current;
                else
                    ctx.Locals.Add(loopVariableName, enumerator.Current);


                foreach (var b in this.Body)
                {
                    b.Render(ctx);
                }
            }

            if (hasShadowedVariable)
                ctx.Locals[loopVariableName] = shadowedVariable;
            else
                ctx.Locals.Remove(loopVariableName);
        }
    }

    internal class ExpressionBlock : Block
    {
        internal ExpressionBlock(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
        public override void Render(Renderer.Context ctx)
        {
            var v = Evaluate(ctx, Expression);
            ctx.Writer.Write(v.StringValue);
        }
    }
    #endregion Block

    #region Clause
    internal abstract class Clause : SyntaxNode
    {
        internal protected Clause(int start, int end)
            : base(start, end) { }
    }

    internal class TextClause : Clause
    {
        internal TextClause(int start, int end)
            : base(start, end) { }
    }

    internal class IfClause : Clause
    {
        internal IfClause(int start, int end,
            Expression test)
            : base(start, end)
        {
            Test = test;
        }

        internal Expression Test { get; }
    }

    internal class ElseIfClause : Clause
    {
        internal ElseIfClause(int start, int end,
            Expression test)
            : base(start, end)
        {
            Test = test;
        }

        internal Expression Test { get; }
    }

    internal class ElseClause : Clause
    {
        internal ElseClause(int start, int end)
            : base(start, end) { }
    }

    internal class EndIfClause : Clause
    {
        internal EndIfClause(int start, int end)
            : base(start, end) { }
    }

    internal class ForClause : Clause
    {
        internal ForClause(int start, int end,
            Expression loopVariable, Expression iterator)
            : base(start, end)
        {
            LoopVariable = loopVariable;
            Iterator = iterator;
        }

        internal Expression LoopVariable { get; }

        internal Expression Iterator { get; }
    }

    internal class EndForClause : Clause
    {
        internal EndForClause(int start, int end)
            : base(start, end) { }
    }

    internal class ExpressionClause : Clause
    {
        internal ExpressionClause(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
    }
    #endregion Clause

    #region Expression
    internal enum UnaryOperator
    {
        Not,       // not A
        Positive,  //   + A
        Negative,  //   - A
    }

    internal enum BinaryOperator
    {
        Pipe,            // A | B
        FunctionCall,    // A ( B )
        Subscript,       // A [ B ]
        MemberAccess,    // A . B

        Or,              // A or B
        And,             // A and B

        Add,             // A + B
        Substract,       // A - B
        Multiply,        // A * B
        DivideFloat,     // A / B
        DivideInteger,   // A // B
        Modulo,          // A % B

        Less,            // A < B
        LessOrEqual,     // A <= B
        Equal,           // A == B
        GreaterOrEqual,  // A >= B
        Greater,         // A > B
        NotEqual,        // A != B
    }

    internal enum ValueType
    {
        String,
        Integer,
        Float,
        Boolean,
        Object,
    }

    internal abstract class Expression : SyntaxNode
    {
        internal protected Expression(int start, int end)
            : base(start, end) { }
    }

    internal class UnaryExpression : Expression
    {
        internal UnaryExpression(int start, int end,
            UnaryOperator @operator, Expression expression)
            : base(start, end)
        {
            Operator = @operator;
            Expression = expression;
        }

        internal UnaryOperator Operator { get; }

        internal Expression Expression { get; }
    }

    internal class BinaryExpression : Expression
    {
        internal BinaryExpression(int start, int end,
            BinaryOperator @operator, Expression left, Expression right)
            : base(start, end)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        internal BinaryOperator Operator { get; }

        internal Expression Left { get; }

        internal Expression Right { get; }
    }

    internal class ListExpression : Expression
    {
        internal ListExpression(int start, int end,
            IEnumerable<Expression> expressions)
            : base(start, end)
        {
            Expressions = expressions;
        }

        internal IEnumerable<Expression> Expressions { get; }
    }

    internal class ParenthesisExpression : Expression
    {
        internal ParenthesisExpression(int start, int end,
            Expression expression)
            : base(start, end)
        {
            Expression = expression;
        }

        internal Expression Expression { get; }
    }

    internal class SymbolExpression : Expression
    {
        internal SymbolExpression(int start, int end,
            string? symbol)
            : base(start, end)
        {
            Symbol = symbol;
        }

        internal string? Symbol { get; }
    }

    internal class LiteralExpression : Expression
    {
        internal LiteralExpression(int start, int end,
            ValueType type,
            string stringValue,
            long integerValue,
            double floatValue,
            bool booleanValue)
            : base(start, end)
        {
            ValueType = type;
            StringValue = stringValue;
            IntegerValue = integerValue;
            FloatValue = floatValue;
            BooleanValue = booleanValue;
        }

        internal ValueType ValueType { get; }

        internal string StringValue { get; }

        internal long IntegerValue { get; }

        internal double FloatValue { get; }

        internal bool BooleanValue { get; }
    }
    #endregion Expression

    #region Token
    internal enum TokenType
    {
        Invalid,

        StatementBegin,      // {%
        StatementEnd,        // %}
        ExpressionBegin,     // {{
        ExpressionEnd,       // }}

        If,                  // if
        ElseIf,              // elif
        Else,                // else
        EndIf,               // endif

        For,                 // for
        In,                  // in
        EndFor,              // 'endfor

        Not,                 // not
        Plus,                // +
        Minus,               // -

        Pipe,                // |
        LeftParenthesis,     // (
        RightParenthesis,    // )
        LeftSquareBracket,   // [
        RightSquareBracket,  // ]
        Dot,                 // .
        Comma,               // ,

        Or,                  // or
        And,                 // and
        Star,                // *
        Slash,               // /
        DoubleSlash,         // //
        Percent,             // %
        Less,                // <
        LessEqual,           // <=
        DoubleEqual,         // ==
        GreaterEqual,        // >=
        Greater,             // >
        ExclamationEqual,    // !=

        Symbol,              // \w+
        String,              // ' \w+ '
        Integer,             // \d+
        Float,               // (\.\d+)|)(\d+)\.(\d+)
        True,                // true
        False,               // false
    }

    internal struct Token
    {
        internal static Token Invalid(int start)
            => new Token(start, start, TokenType.Invalid);

        internal Token(int start, int end, TokenType tokenType)
        {
            Start = start;
            End = end;
            TokenType = tokenType;
        }

        internal int Start { get; }

        internal int End { get; }

        internal TokenType TokenType { get; }

        internal bool IsInvalid => TokenType == TokenType.Invalid;

        internal bool IsCompareOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Less:
                    case TokenType.LessEqual:
                    case TokenType.DoubleEqual:
                    case TokenType.GreaterEqual:
                    case TokenType.Greater:
                    case TokenType.ExclamationEqual:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsTermOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus:
                    case TokenType.Minus:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsFactorOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Star:
                    case TokenType.Slash:
                    case TokenType.DoubleSlash:
                    case TokenType.Percent:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal bool IsAtomExpressionOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus:
                    case TokenType.Minus:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal BinaryOperator AsBinaryOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Less: return BinaryOperator.Less;
                    case TokenType.LessEqual: return BinaryOperator.LessOrEqual;
                    case TokenType.DoubleEqual: return BinaryOperator.Equal;
                    case TokenType.GreaterEqual: return BinaryOperator.GreaterOrEqual;
                    case TokenType.Greater: return BinaryOperator.Greater;
                    case TokenType.ExclamationEqual: return BinaryOperator.NotEqual;

                    case TokenType.Plus: return BinaryOperator.Add;
                    case TokenType.Minus: return BinaryOperator.Substract;

                    case TokenType.Star: return BinaryOperator.Multiply;
                    case TokenType.Slash: return BinaryOperator.DivideFloat;
                    case TokenType.DoubleSlash: return BinaryOperator.DivideInteger;
                    case TokenType.Percent: return BinaryOperator.Modulo;

                    default:
                        throw new InvalidOperationException("Cannot convert to binary operator");
                }
            }
        }

        internal UnaryOperator AsUnaryOperator
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.Plus: return UnaryOperator.Positive;
                    case TokenType.Minus: return UnaryOperator.Negative;

                    default:
                        throw new InvalidOperationException("Cannot convert to unary operator");
                }
            }
        }
    }
    #endregion Token
}
