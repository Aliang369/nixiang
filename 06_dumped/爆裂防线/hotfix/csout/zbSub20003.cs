using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub20003 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
		}
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
	}

	protected override void Update()
	{
		base.Update();
		bool flag = false;
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if ((long)monster.Value.curHp > 0 && monster.Value.monsterIndex40151.monsterIndex40151)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			ReturnToPool();
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null && component.monsterIndex40151.monsterIndex40151 && (long)component.curHp > 0)
		{
			component.SetCurHp(0L);
			zbCombatSystem.Instance.monster3006Reborn.monster3006PartNum++;
			component.monsterIndex40151.monsterSkill.TriggerDieSkill(null);
		}
	}
}
