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
            var argToAdd = macro.ArgNames[key];
            
            if (index < args.Count)
            {
                var arg = args[index];
                if (arg is IdBlock idBlock)
                {
                   argToAdd =  new ValueBlock(idBlock.GetValue(context));
                }
                else
                {
                    argToAdd = arg;
                }
            }
            
            variables.Add(key, argToAdd);
        }

        var macroCotext = new Context()
        {
            Macros = context.Macros,
            Variables = variables,
        };
        
        macro.Execute(macroCotext);
        RenderableBlock.Render(context, this, macroCotext.Builder.ToString());
    }
}