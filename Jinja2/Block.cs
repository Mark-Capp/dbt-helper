namespace Jinja2;

public interface IBlock
{
    public bool ShouldStripNewLines { get; set; }
}

public abstract class Block : IBlock
{
    public bool ShouldStripNewLines { get; set; }
}