using System;
using System.Collections.Generic;

public class zbMonsterSkillIndex40402 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex40402.skillAttackType = (int)monsterSkillRecord.param[0];
		monster.monsterIndex40402.monsterBossTid = (int)monsterSkillRecord.param[1];
		monster.monsterIndex40402.monsterIndex40402 = true;
		monster.monsterIndex40402.monsterSkill = this;
	}

	public override void TriggerDieSkill(Action callback)
	{
		m_callback = callback;
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (monster.Value.monsterRecord.id != base.monster.monsterIndex40402.monsterBossTid)
			{
				continue;
			}
			if (monster.Value.monsterSkillIndex3014 == 2)
			{
				if (monster.Value.monster30142Class != null)
				{
					monster.Value.monster30142Class.TriggerDaze();
				}
			}
			else if (monster.Value.monsterSkillIndex3014 == 3 && monster.Value.monster30143Class != null)
			{
				monster.Value.monster30143Class.TriggerDaze();
			}
		}
		m_callback?.Invoke();
	}

	public override void ClearList()
	{
	}
}
