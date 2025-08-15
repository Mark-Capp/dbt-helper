namespace Jinja2;

public class MacroCallBlock(
    string name,
    List<ExpressionBlock> args): Block, IRender
{
    
    public void Render(Context context)
    {
        if (!context.Macros.TryGetValue(name, out var macro))
        {
            return;
        }
        
        var variables = new Dictionary<string, ExpressionBlock>();
        for (var index = 0; index < macro.ArgNames.Keys.Count; index++)
        {
            var key = macro.ArgNames.Keys.ElementAt(index);
            variables.Add(key, index >= args.Count ? macro.ArgNames[key] : args[index]);
        }

        var macroCotext = new Context()
        {
            Macros = context.Macros,
            Variables = variables,
            Content = string.Empty
        };
        
        macro.Execute(macroCotext);
        context.Content += macroCotext.Content;
    }
}