using UnityEngine;

public class zbMonsterSkillIndex40462 : zbMonsterSkillBase
{
	private bool isActioned;

	private float triggerTime;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex40462.distance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex40462.lessMass = (int)monsterSkillRecord.param[1];
		monster.monsterIndex40462.monsterTid = (int)monsterSkillRecord.param[2];
		monster.monsterIndex40462.triggerTime = monsterSkillRecord.param[3];
		triggerTime = 0f;
		isActioned = false;
	}

	public override void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (isActioned)
		{
			return;
		}
		triggerTime += Time.deltaTime;
		if (!(triggerTime >= monster.monsterIndex40462.triggerTime))
		{
			return;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, monsterRangeAllList, monster.monsterIndex40462.distance, monster.GetUUID());
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			if ((long)monsterRangeAll.curHp > 0 && monsterRangeAll.GetTid() != monster.GetTid() && monsterRangeAll.GetMass() <= (float)monster.monsterIndex40462.lessMass)
			{
				isActioned = true;
				monsterRangeAll.SetCurHp(0L);
				monster.SetCurHp(0L);
				PlayTackAnimation(1, flag: false);
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex40462.monsterTid, ((Component)monster).transform.position, ((Component)monster).transform.parent);
				break;
			}
		}
	}
}
