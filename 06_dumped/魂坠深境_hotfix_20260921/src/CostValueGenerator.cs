public class CostValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.spellInfo == null || eve.triggerType != GameEvent.TriggerType.Real)
		{
			return 0;
		}
		return eve.spellInfo.realCostValue;
	}
}
