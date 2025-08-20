using System.Runtime.CompilerServices;

namespace Jinja2;

public class ForBlock(
    string loopVarName,
    ExpressionBlock iterable,
    IBlock[] body
) : Block, IRender
{
    public void Render(Context context)
    {
        // Evaluate the iterable
        var value = iterable.GetValue(context);

        // Try to iterate; handle null and non-iterables
        var enumerable = AsEnumerable(value);

        // Preserve previous value of the loop variable (if any)
        context.Variables.TryGetValue(loopVarName, out var previous);


        if (enumerable is not null)
        {
            var array = enumerable.ToArray();
            var classBlock = new ClassBlock(new Dictionary<string, ExpressionBlock>
            {
                { "first", new IntBlock(1) },
                { "last", new IntBlock(0) },
                { "length", new IntBlock(array.Length) },
            });
            context.Variables.Add("loop", classBlock);
            
            for(var i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if(i != 0)
                    classBlock.SetProperty("first", new IntBlock(0));
                
                if(i == array.Length - 1)
                    classBlock.SetProperty("last", new IntBlock(1));
                
                
                // Assign the current item into the loop variable (wrap as ExpressionBlock if needed)
                context.Variables[loopVarName] = item as ExpressionBlock ?? new ValueBlock(item);
                RenderBody(context, body);
            }
        }

        // Restore previous value or remove if none
        if (previous is not null)
            context.Variables[loopVarName] = previous;
        else
            context.Variables.Remove(loopVarName);
    }

    private static IEnumerable<object?>? AsEnumerable(object? source)
    {
        switch (source)
        {
            case null:
                return null;
            case string s:
                // Strings are iterable by character; but typical Jinja treats them as sequences; here, we iterate chars
                return s.Select(c => (object?)c.ToString());
            case System.Collections.IEnumerable nonGeneric:
                return nonGeneric.Cast<object?>();
            default:
                // Try to handle common IEnumerable<T>
                var ienum = source.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (ienum != null)
                {
                    var castMethod = typeof(Enumerable).GetMethod("Cast")?.MakeGenericMethod(typeof(object));
                    var toList = typeof(Enumerable).GetMethod("ToList")?.MakeGenericMethod(typeof(object));
                    if (castMethod != null && toList != null)
                    {
                        var casted = castMethod.Invoke(null, new[] { source });
                        var list = toList.Invoke(null, new[] { casted }) as List<object?>;
                        return list;
                    }
                }
                return null;
        }
    }

    private void RenderBody(Context context, IEnumerable<IBlock> blocks)
    {
        foreach (var block in blocks)
        {
            block.ShouldStripNewLines = ShouldStripNewLines;
            if (block is IRender render)
                render.Render(context);
        }
    }
}
