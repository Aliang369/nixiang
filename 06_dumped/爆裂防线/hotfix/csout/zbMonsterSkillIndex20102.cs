using UnityEngine;

public class zbMonsterSkillIndex20102 : zbMonsterSkillBase
{
	private float interval;

	public override void RefreshProperty()
	{
		interval = 0f;
		base.RefreshProperty();
		monster.monsterIndex20102.skillIndexFlag = true;
		monster.monsterIndex20102.distance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex20102.interval = monsterSkillRecord.param[1];
		monster.monsterIndex20102.hpPercent = monsterSkillRecord.param[2];
		monster.monsterIndex20102.attrMonsterType1 = (int)monsterSkillRecord.param[3];
		monster.monsterIndex20102.attrPercent1 = monsterSkillRecord.param[4];
		monster.monsterIndex20102.attrMonsterType2 = (int)monsterSkillRecord.param[5];
		monster.monsterIndex20102.attrPercent2 = monsterSkillRecord.param[6];
	}

	public override void Update()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (monster.monsterIndex20102.skillIndexFlag && monster.monsterIndex20102.distance <= ((Component)monster).transform.position.y - monster.combatData.defense.GetAlertPositionY())
		{
			interval += Time.deltaTime;
			if (interval >= monster.monsterIndex20102.interval)
			{
				monster.ChangeHp(null, null, (long)(monster.monsterIndex20102.hpPercent * (float)(long)monster.maxHp));
				interval = 0f;
			}
		}
	}

	public override void ClearList()
	{
	}
}
