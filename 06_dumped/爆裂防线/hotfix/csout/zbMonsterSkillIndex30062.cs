using System;
using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex30062 : zbMonsterSkillBase
{
	public List<int> callMonsterUUIDList = new List<int>();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex30062.monsterSkill = this;
		for (int i = 1; i < monsterSkillRecord.param.Length; i++)
		{
			float num = monsterSkillRecord.param[i];
			monster.monsterIndex30062.monsterTidList.Add((int)num);
		}
		monster.monsterIndex30062.distance = monsterSkillRecord.param[0] / 100f;
	}

	public override void TriggerDieSkill(Action callback)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		m_callback = callback;
		if (zbCombatSystem.Instance.monster3006Reborn.monster3006CanRebornNum >= 1)
		{
			m_callback?.Invoke();
			return;
		}
		zbCombatSystem.Instance.monster3006Reborn.monster3006SkillSub = 1;
		zbCombatSystem.Instance.monster3006Reborn.monster3006PositionX = ((Component)monster).transform.position.x;
		zbCombatSystem.Instance.monster3006Reborn.monster3006PositionY = ((Component)monster).transform.position.y;
		zbHero zbHero2 = monster.combatData.masterTeam.heroList[0];
		zbSkillSystem.Instance.CreateSubSkill(20003, zbHero2, null, zbHero2.heroAttr.skillAttrList, ((Component)monster).transform.position, Vector2.up);
		if (monster.monsterIndex30062.monsterTidList.Count == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex30062.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num = 0;
			foreach (int monsterTid in monster.monsterIndex30062.monsterTidList)
			{
				num = ((num >= 8) ? 7 : num);
				combatData.monsters.SpawnMonsterAtPosition(monsterTid, zbMonsterSkillBase.vector3s[num] * monster.monsterIndex30062.distance + ((Component)monster).transform.position, ((Component)monster).transform.parent);
				num++;
			}
		}
		zbCombatSystem.Instance.monster3006Reborn.monster3006CanRebornNum++;
		m_callback?.Invoke();
	}

	public override void ClearList()
	{
		monster.monsterIndex30062.monsterTidList.Clear();
		monster.monsterIndex30062.monsterSkill = null;
	}
}
