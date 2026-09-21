public class CasterSkillPropertyValueGenerator : ValueGenerator
{
	public SkillDef.SkillPropertyType type;

	public override int? GenerateValue(GameEvent eve)
	{
		return GeneratePropertyValue(eve, type);
	}

	public int? GeneratePropertyValue(GameEvent eve, SkillDef.SkillPropertyType type)
	{
		int? num = eve.CasterSkill?.GetSkillPropertyValue(type);
		if (num.HasValue && eve.gameState?.gameEventManager != null)
		{
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerSkillPropertyValueResolve(eve.CasterSkill, type, eve.triggerType);
			if (resolveInfo != null)
			{
				num = (int)((1f + (float)resolveInfo.deltaPercent / 100f) * (float?)num).Value;
				num += resolveInfo.deltaValue;
			}
		}
		return num;
	}
}
