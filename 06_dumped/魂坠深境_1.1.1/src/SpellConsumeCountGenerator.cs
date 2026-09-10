public class SpellConsumeCountGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.spellInfo == null)
		{
			return null;
		}
		if (eve.triggerType != GameEvent.TriggerType.Real && eve.spellInfo.addCostCount <= 0)
		{
			return null;
		}
		return eve.spellInfo.addCostCount;
	}
}
