using System;
using System.Collections.Generic;

public sealed class DancerCharmBladeDamageGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		GameSkill gameSkill = eve?.CasterSkill;
		if (gameSkill == null)
		{
			return 0;
		}
		Dictionary<string, object> tempValues = gameSkill.tempValues;
		if (tempValues == null || !tempValues.TryGetValue("DancerCharmBladeSpellMovementAbility.AccumulatedDamage", out var value))
		{
			return 0;
		}
		return Convert.ToInt32(value);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(GenerateValue(eve))
		};
	}
}
