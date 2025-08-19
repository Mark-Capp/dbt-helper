namespace Jinja2;

public interface IBlock
{
    public bool ShouldStripNewLines { get; set; }
}

public abstract class Block : IBlock
{
    public bool ShouldStripNewLines { get; set; }

    // Allow a single block to be used where a list is expected (e.g., AddRange)
    public static implicit operator List<IBlock>(Block block) => new List<IBlock> { block };
}