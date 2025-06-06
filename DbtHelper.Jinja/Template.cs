using System;

namespace DbtHelper.Jinja
{
    public class Template
    {
        private Template(Renderer renderer)
        {
            _renderer = renderer;
        }

        public static Template FromString(string str)
        {
            ArgumentNullException.ThrowIfNull(str);

            var parser = new Parser(str);
            var blocks = parser.Parse();
            var renderer = new Renderer(str, blocks);
            var template = new Template(renderer);
            return template;
        }

        public string Render(dynamic globals)
        {
            return _renderer.Render(globals);
        }

        private readonly Renderer _renderer;
    }
}
