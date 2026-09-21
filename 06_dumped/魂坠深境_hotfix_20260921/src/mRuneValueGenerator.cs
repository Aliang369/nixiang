public class mRuneValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		return eve.CasterRune?.value ?? 0;
	}
}
