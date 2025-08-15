namespace Jinja2;

public interface IBlock;
public abstract class Block
{
    public bool ShouldStripNewLines { get; set; }
}