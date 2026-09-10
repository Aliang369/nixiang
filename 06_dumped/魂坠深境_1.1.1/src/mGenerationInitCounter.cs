public class mGenerationInitCounter : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.CasterItem?.resetInitCounter ?? 0;
	}
}
