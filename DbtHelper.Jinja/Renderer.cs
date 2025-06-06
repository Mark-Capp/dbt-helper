using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DbtHelper.Jinja;

internal class Renderer
{
    private readonly IEnumerable<Block> _blocks;
    private readonly string _template;
        
    internal Renderer(string template, IEnumerable<Block> blocks)
    {
        Debug.Assert(template != null);
        Debug.Assert(blocks != null);

        _blocks = blocks;
        _template = template;
    }

    internal string Render(object globals)
    {
        var buffer = new StringBuilder();
        using (var writer = new StringWriter(buffer))
        {
            var context = new Context(writer, globals, _template);
            
            foreach (var b in _blocks)
                RenderBlock(context, b);
        }

        return buffer.ToString();
    }

    private static void RenderBlock(Context ctx, Block block)
    {
        switch (block)
        {
            case TextBlock:
            case IfStatementBlock:
            case ForStatementBlock:
            case ExpressionBlock:
                block.Render(ctx);
                break;

            default:
                throw new InvalidOperationException("Unknown block type to render");
        }
    }


    internal class Context
    {
        internal Context(TextWriter writer, object globals, string template)
        {
            Writer = writer;
            Globals = globals;
            Locals = new Dictionary<string?, object>();
            Template = template;

            Filters = PredefinedFilters;
        }

        internal TextWriter Writer { get; }
        public string Template { get; }

        internal object Globals { get; }

        internal IDictionary<string?, object> Locals { get; }

        internal IDictionary<string?, MethodInfo> Filters { get; }

        private static readonly IDictionary<string?, MethodInfo> PredefinedFilters
            = new Dictionary<string?, MethodInfo>
            {
                { "replace",  typeof(Filters).GetMethod("Replace", BindingFlags.NonPublic | BindingFlags.Static)}
            };
    }


}
    
internal class Filters
{
    internal static string Replace(string str, string oldValue, string newValue)
    {
        return str.Replace(oldValue, newValue);
    }
}
    
internal struct ExpressionValue
{
    internal static readonly ExpressionValue Empty = new("");

    internal ExpressionValue(string stringValue)
    {
        ValueType = ValueType.String;
        StringValue = stringValue;
        long.TryParse(stringValue, out var integerValue);
        IntegerValue = integerValue;
        double.TryParse(stringValue, out var floatValue);
        FloatValue = floatValue;
        BooleanValue = !string.IsNullOrEmpty(stringValue);
        ObjectValue = stringValue;
    }

    internal ExpressionValue(long integerValue)
    {
        ValueType = ValueType.Integer;
        StringValue = integerValue.ToString();
        IntegerValue = integerValue;
        FloatValue = integerValue;
        BooleanValue = integerValue != 0;
        ObjectValue = integerValue;
    }

    internal ExpressionValue(double floatValue)
    {
        ValueType = ValueType.Float;
        StringValue = floatValue.ToString();
        IntegerValue = (long)floatValue;
        FloatValue = floatValue;
        BooleanValue = floatValue != 0.0;
        ObjectValue = floatValue;
    }

    internal ExpressionValue(bool booleanValue)
    {
        ValueType = ValueType.Boolean;
        StringValue = booleanValue.ToString();
        IntegerValue = booleanValue ? 1 : 0;
        FloatValue = booleanValue ? 1.0 : 0.0;
        BooleanValue = booleanValue;
        ObjectValue = booleanValue;
    }

    internal ExpressionValue(object? objectValue)
    {
        ValueType = ValueType.Object;
        var stringValue = objectValue != null ? objectValue.ToString() : "";
        StringValue = stringValue;
        long.TryParse(stringValue, out var integerValue);
        IntegerValue = integerValue;
        double.TryParse(stringValue, out var floatValue);
        FloatValue = floatValue;
        BooleanValue = !string.IsNullOrEmpty(stringValue);
        ObjectValue = objectValue;
    }

    internal ValueType ValueType { get; }

    internal string StringValue { get; }

    internal long IntegerValue { get; }

    internal double FloatValue { get; }

    internal bool BooleanValue { get; }

    internal object ObjectValue { get; }
}