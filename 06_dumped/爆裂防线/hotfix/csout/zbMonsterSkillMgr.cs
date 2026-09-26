using System.Collections.Generic;

public class zbMonsterSkillMgr
{
	public zbMonster monster;

	public List<zbMonsterSkillBase> monsterSkillList { get; set; }

	public void Init(zbMonster _monster, bool _giveExp)
	{
		monster = _monster;
		monsterSkillList = new List<zbMonsterSkillBase>();
		CreateSkill(monster, monster.monsterRecord.monsterSkill1);
		CreateSkill(monster, monster.monsterRecord.monsterSkill2);
		CreateSkill(monster, monster.monsterRecord.monsterSkill3);
		if (monster.isEliteBoss)
		{
			CreateSkill(monster, 10001);
		}
		if (!_giveExp)
		{
			return;
		}
		int legendaryEffect = zbCombatSystem.Instance.battleInfo.MonsterInfo.ExtraInfo.LegendaryEffect;
		if (legendaryEffect > 0)
		{
			zbLegendaryEffectCsvRecord recordByKey = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(legendaryEffect);
			if (recordByKey != null)
			{
				CreateSkill(monster, recordByKey.mstSkillId);
			}
		}
		if (!zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		int num = 0;
		num = ((monster.combatData.dataIndex != 1) ? zbCombatSystem.Instance.combatData.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId : zbCombatSystem.Instance.combatData2.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId);
		zbFootholdCsvRecord recordByKey2 = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(num);
		if (recordByKey2 != null && recordByKey2.effectId > 0)
		{
			zbLegendaryEffectCsvRecord recordByKey3 = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(recordByKey2.effectId);
			if (recordByKey3 != null)
			{
				CreateSkill(monster, recordByKey3.mstSkillId);
			}
		}
	}

	public void CreateSkill(zbMonster targetMonster, int skillId)
	{
		zbMonsterSkillBase zbMonsterSkillBase2 = zbSkillSystem.Instance.monsterIndexRegister.CreateMonsterSkill(skillId);
		if (zbMonsterSkillBase2 != null)
		{
			zbMonsterSkillBase2.Init(targetMonster, skillId);
			monsterSkillList.Add(zbMonsterSkillBase2);
		}
	}

	public void Update()
	{
		foreach (zbMonsterSkillBase monsterSkill in monsterSkillList)
		{
			monsterSkill.Update();
		}
	}

	public void Free()
	{
		foreach (zbMonsterSkillBase monsterSkill in monsterSkillList)
		{
			monsterSkill.ClearList();
		}
		monsterSkillList.Clear();
	}
}
