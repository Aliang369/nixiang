public class MInfluenceValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.CasterInfluence == null)
		{
			return null;
		}
		return eve.CasterInfluence.influenceValue;
	}
}
