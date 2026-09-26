using UnityEngine;

public class zbMonsterSkillIndex10462 : zbMonsterSkillBase
{
	private int monsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10462.distance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex10462.monsterTid1 = (int)monsterSkillRecord.param[1];
		monster.monsterIndex10462.monsterTid1 = (int)monsterSkillRecord.param[2];
		monsterUUID = monster.GetUUID();
	}

	public override void Update()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuffStop())
		{
			return;
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById == (Object)null || (long)monsterById.curHp <= 0)
		{
			return;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monsterById).transform.position, monsterRangeAllList, monsterById.monsterIndex10462.distance, monsterById.GetUUID());
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			if ((long)monsterRangeAll.curHp >= 0 && monsterRangeAll.monsterRecord.id == monsterById.monsterIndex10462.monsterTid1)
			{
				monsterRangeAll.SetCurHp(0L);
				combatData.monsters.SpawnMonsterAtPosition(monsterById.monsterIndex10462.monsterTid2, ((Component)monsterById).transform.position, ((Component)monsterById).transform.parent);
				break;
			}
		}
	}
}
