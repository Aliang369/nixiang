using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbCombatResult
{
	public void Init()
	{
	}

	public void Free()
	{
	}

	public BattleResultInfo CalcFightResult()
	{
		BattleResultInfo battleResultInfo = new BattleResultInfo
		{
			AttackerTeamInfo = new BattleResultSideTeamInfo
			{
				SideInfo = new BattleResultSideInfo(),
				MasterTeamInfo = new BattleResultTeamInfo(),
				HelperTeamInfo = new BattleResultTeamInfo()
			},
			DefenderTeamInfo = new BattleResultSideTeamInfo
			{
				SideInfo = new BattleResultSideInfo(),
				MasterTeamInfo = new BattleResultTeamInfo(),
				HelperTeamInfo = new BattleResultTeamInfo()
			},
			DamageWatchInfo = new BattleResultDamageWatchInfo()
		};
		battleResultInfo.UUID = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.UUID;
		battleResultInfo.BattleType = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType;
		battleResultInfo.TimeUsed = (int)((float)zbCombatSystem.Instance.accumTime + 1f);
		if (zbCombatSystem.Instance.IsKillBoss())
		{
			battleResultInfo.ResultScore = EBattleResultScore.Win3;
			battleResultInfo.AttackerTeamInfo.SideInfo.Level = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel;
			battleResultInfo.AttackerTeamInfo.SideInfo.Round = zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList.Count;
			battleResultInfo.AttackerTeamInfo.SideInfo.DefensePct = (float)(long)zbCombatSystem.Instance.combatData.defense.curHp / (float)(long)zbCombatSystem.Instance.combatData.defense.maxHp * 100f;
			battleResultInfo.AttackerTeamInfo.SideInfo.KillAll = EBoolState.True;
			battleResultInfo.AttackerTeamInfo.SideInfo.KillCount = zbCombatSystem.Instance.combatData.combatExp.monsterTotalCount;
			battleResultInfo.AttackerTeamInfo.SideInfo.KillBoss = EBoolState.True;
			battleResultInfo.AttackerTeamInfo.MasterTeamInfo.KillCount = 0;
		}
		else
		{
			battleResultInfo.ResultScore = EBattleResultScore.Fail;
			battleResultInfo.AttackerTeamInfo.SideInfo.Level = Math.Min(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel, (int)zbCombatSystem.Instance.combatData.combatExp.curLevel + zbCombatSystem.Instance.battleInfo.BattleBaseInfo.InitLevel);
			battleResultInfo.AttackerTeamInfo.SideInfo.Round = zbCombatSystem.Instance.combatData.combatExp.curLevel;
			battleResultInfo.AttackerTeamInfo.SideInfo.DefensePct = (float)(long)zbCombatSystem.Instance.combatData.defense.curHp / (float)(long)zbCombatSystem.Instance.combatData.defense.maxHp * 100f;
			battleResultInfo.AttackerTeamInfo.SideInfo.KillAll = (zbCombatSystem.Instance.combatData.combatExp.IsKillAll() ? EBoolState.True : EBoolState.False);
			battleResultInfo.AttackerTeamInfo.SideInfo.KillCount = zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount;
			battleResultInfo.AttackerTeamInfo.SideInfo.KillBoss = EBoolState.False;
		}
		if (zbCombatSystem.Instance.combatData.helperTeam != null)
		{
			CaclBattleResultTeamInfo(battleResultInfo.AttackerTeamInfo.HelperTeamInfo, zbCombatSystem.Instance.combatData.helperTeam);
		}
		CaclBattleResultTeamInfo(battleResultInfo.AttackerTeamInfo.MasterTeamInfo, zbCombatSystem.Instance.combatData.masterTeam);
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			if (zbCombatSystem.Instance.combatData2.helperTeam != null)
			{
				CaclBattleResultTeamInfo(battleResultInfo.DefenderTeamInfo.HelperTeamInfo, zbCombatSystem.Instance.combatData2.helperTeam);
			}
			CaclBattleResultTeamInfo(battleResultInfo.DefenderTeamInfo.MasterTeamInfo, zbCombatSystem.Instance.combatData2.masterTeam);
			battleResultInfo.DefenderTeamInfo.SideInfo.Level = Math.Min(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel, (int)zbCombatSystem.Instance.combatData2.combatExp.curLevel + zbCombatSystem.Instance.battleInfo.BattleBaseInfo.InitLevel);
			battleResultInfo.DefenderTeamInfo.SideInfo.Round = zbCombatSystem.Instance.combatData2.combatExp.curLevel;
			battleResultInfo.DefenderTeamInfo.SideInfo.DefensePct = (float)(long)zbCombatSystem.Instance.combatData2.defense.curHp / (float)(long)zbCombatSystem.Instance.combatData2.defense.maxHp * 100f;
			battleResultInfo.DefenderTeamInfo.SideInfo.KillAll = (zbCombatSystem.Instance.combatData2.combatExp.IsKillAll() ? EBoolState.True : EBoolState.False);
			battleResultInfo.DefenderTeamInfo.SideInfo.KillCount = zbCombatSystem.Instance.combatData2.combatExp.monsterKilledCount;
			battleResultInfo.DefenderTeamInfo.SideInfo.KillBoss = EBoolState.False;
		}
		battleResultInfo.DamageWatchInfo.DamageInit = zbCombatSystem.Instance.qiangDamageInit;
		battleResultInfo.DamageWatchInfo.HurtMin = zbCombatSystem.Instance.qiangHurtMin;
		battleResultInfo.DamageWatchInfo.HurtMax = zbCombatSystem.Instance.qiangHurtMax;
		battleResultInfo.DamageWatchInfo.MonsterId = zbCombatSystem.Instance.qiangFirstMonsterId;
		zbHero heroBySkillType = zbCombatSystem.Instance.combatData.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
		if ((Object)(object)heroBySkillType != (Object)null)
		{
			battleResultInfo.DamageWatchInfo.DamageFinal = (int)heroBySkillType.GetAttrValueByType(EAttrSkillType.Damage);
		}
		return battleResultInfo;
	}

	public void CaclBattleResultTeamInfo(BattleResultTeamInfo resultTeamInfo, zbCombatTeam team)
	{
		resultTeamInfo.RoleBaseInfo = team.battleTeamInfo.RoleBaseInfo;
		foreach (KeyValuePair<int, long> heroDamageStat in team.combatStat.heroDamageStats)
		{
			BattleResultHeroDetail battleResultHeroDetail = new BattleResultHeroDetail();
			battleResultHeroDetail.HeroTid = heroDamageStat.Key;
			battleResultHeroDetail.Hurt = heroDamageStat.Value;
			resultTeamInfo.HeroList.Add(battleResultHeroDetail);
		}
		foreach (zbModBase mod in team.modMgr.modList)
		{
			if (mod.isUserSelect)
			{
				BattleResultModDetail battleResultModDetail = new BattleResultModDetail();
				battleResultModDetail.Level = mod.level;
				battleResultModDetail.ModId = mod.modRecord.id;
				resultTeamInfo.ModList.Add(battleResultModDetail);
			}
		}
	}
}
