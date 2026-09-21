public class LoopIndexGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.CastNodeLoopIndex;
	}
}
