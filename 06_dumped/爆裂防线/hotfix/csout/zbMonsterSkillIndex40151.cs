using System;
using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex40151 : zbMonsterSkillBase
{
	private static bool isPartCall;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex40151.monsterIndex40151 = true;
		monster.monsterIndex40151.monsterSkill = this;
		monster.monsterIndex40151.monsterTid = (int)monsterSkillRecord.param[0];
	}

	public override void TriggerDieSkill(Action callback)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
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
			if (zbCombatSystem.Instance.monster3006Reborn.monster3006PartNum > 0 && !isPartCall)
			{
				zbMonster zbMonster2 = combatData.monsters.SpawnMonsterAtPosition(base.monster.monsterIndex40151.monsterTid, new Vector3(zbCombatSystem.Instance.monster3006Reborn.monster3006PositionX, zbCombatSystem.Instance.monster3006Reborn.monster3006PositionY), ((Component)base.monster).transform.parent);
				if ((Object)(object)zbMonster2 != (Object)null)
				{
					zbMonster2.SetCurHp((int)((float)zbCombatSystem.Instance.monster3006Reborn.monster3006PartNum / 4f * (float)(long)zbMonster2.maxHp));
					isPartCall = true;
				}
			}
			zbSkillBase zbSkillBase2 = base.monster.combatData.skillPoolMgr.FindSkill(20003);
			if ((Object)(object)zbSkillBase2 != (Object)null)
			{
				if ((Object)(object)zbSkillBase2.m_skeletonAnimation != (Object)null)
				{
					zbSkillBase2.m_skeletonAnimation.AnimationState.SetAnimation(0, "fuhuo", false);
				}
				zbSkillBase2.ReturnToPool(0.5f);
			}
			zbCombatSystem.Instance.monster3006Reborn.monster3006SkillSub = 0;
		}
		callback?.Invoke();
	}

	public override void ClearList()
	{
		monster.monsterIndex40151.monsterIndex40151 = false;
		monster.monsterIndex40151.monsterSkill = null;
	}
}
