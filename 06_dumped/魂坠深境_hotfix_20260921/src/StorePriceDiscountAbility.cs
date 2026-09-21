using System.Collections.Generic;
using UnityEngine;

public class StorePriceDiscountAbility : Ability
{
	public SkillDef.SkillPropertyType discountProperty = SkillDef.SkillPropertyType.Strength;

	public int GetDiscountPercent(GameSkill skill)
	{
		if (skill == null)
		{
			return 0;
		}
		return Mathf.Clamp(skill.GetSkillPropertyValue(discountProperty), 0, 100);
	}

	public static int ApplyToPrice(int price, int discountPercent)
	{
		if (price <= 0)
		{
			return price;
		}
		int num = Mathf.Clamp(discountPercent, 0, 100);
		long num2 = (long)price * (long)(100 - num);
		return Mathf.Max(1, (int)((num2 + 99) / 100));
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		GameSkill gameSkill = eve?.CasterSkill;
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>((gameSkill == null) ? ((int?)null) : new int?(GetDiscountPercent(gameSkill)))
		};
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield break;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
	}
}
