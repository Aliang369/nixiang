using UnityEngine;

public class zbMonsterSkillIndex20135 : zbMonsterSkillBase
{
	private float interval;

	public override void RefreshProperty()
	{
		interval = 0f;
		base.RefreshProperty();
		monster.monsterIndex20135.skillIndexFlag = true;
		monster.monsterIndex20135.monsterTid = (int)monsterSkillRecord.param[0];
		monster.monsterIndex20135.hpPercent = monsterSkillRecord.param[1];
	}

	public override void Update()
	{
		if (!monster.monsterIndex20135.skillIndexFlag)
		{
			return;
		}
		interval += Time.deltaTime;
		if (interval >= 1f)
		{
			if (monster.combatData.monsters.HaveMonsterByTid(monster.monsterIndex20135.monsterTid))
			{
				monster.ChangeHp(null, null, (long)(monster.monsterIndex20135.hpPercent * (float)(long)monster.maxHp));
			}
			interval = 0f;
		}
	}

	public override void ClearList()
	{
	}
}
