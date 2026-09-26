using System.Collections.Generic;
using UnityEngine;

public class zbBuffIndex13 : zbBuffBase
{
	private bool isBuffIndex13End;

	private List<zbMonster> monsterRandomList = new List<zbMonster>();

	public override void RefreshProperty()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.attackCount = 0;
		}
		isBuffIndex13End = false;
		durationTime = 999999f;
	}

	public override void TriggerDieAction()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.zero;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			val = Vector2.op_Implicit(((Component)monsterById).transform.position);
		}
		skillBase.combatData.monsters.GetRangeAllMonsterList(Vector2.op_Implicit(val), monsterRandomList, 2f);
		foreach (zbMonster monsterRandom in monsterRandomList)
		{
			zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monsterRandom.GetUUID(), buffRecord.id);
		}
		isBuffIndex13End = true;
	}

	public override bool IsEnd()
	{
		if (base.IsEnd())
		{
			return true;
		}
		if (isBuffIndex13End)
		{
			return true;
		}
		return false;
	}
}
