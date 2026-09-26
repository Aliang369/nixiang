using System;
using Proto.Enums;
using Proto.Types;

public class zbCombatViolent
{
	public stViolentData violentData;

	public void Init(BattleTeamInfo battleTeamInfo)
	{
		violentData.violentOffer = 1f;
		violentData.violentRateAdd = 0f;
		violentData.violentDamageAdd = 0f;
		violentData.violentRate = 0.3f;
		violentData.violentDamage = 0f;
		foreach (BattleTeamAttrDetail attr in battleTeamInfo.AttrList)
		{
			switch (attr.Type)
			{
			case EAttrTeamType.CritRate:
				violentData.violentRate = ((attr.Value > 0.01f) ? attr.Value : 0.01f);
				break;
			case EAttrTeamType.CritDamage:
				violentData.violentDamage = attr.Value;
				break;
			}
		}
	}

	public float CalcViolent(zbSkillBase skillBase, EAttrSkillType attrSkillType, zbMonster monster)
	{
		float num = 0f;
		foreach (stModIndex1064 item in skillBase.hero.team.modIndex1064)
		{
			num += item.violentRateAdd;
		}
		float num2 = Math.Max(violentData.violentRate + num, 0.001f);
		float num3 = 1f / num2;
		bool flag = false;
		if ((double)num2 > 0.99)
		{
			flag = true;
		}
		else if ((double)num2 < 0.01)
		{
			flag = false;
		}
		else if (violentData.violentOffer >= num3)
		{
			flag = true;
			violentData.violentOffer = violentData.violentOffer + 1f - num3;
		}
		else
		{
			flag = false;
			violentData.violentOffer++;
		}
		if (skillBase.hero.team.ExtraProperties[ETeamExtra.GemMod1054] > 0f && monster.curHp >= monster.maxHp)
		{
			flag = true;
		}
		if (flag)
		{
			return violentData.violentDamage;
		}
		return 0f;
	}
}
