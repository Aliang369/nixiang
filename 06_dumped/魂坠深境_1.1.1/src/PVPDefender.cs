using System;
using System.Collections.Generic;
using System.Linq;

public class PVPDefender
{
	[SaveField(null, null)]
	public string guid;

	[SaveField(null, null)]
	public string sourcePlayerId;

	[SaveField(null, null)]
	public string sourcePlayerName;

	[SaveField(null, null)]
	public string defenderName;

	[SaveField(null, null)]
	public EnemyAssetRef enemyAvatarRef;

	[SaveField(null, null)]
	public int remainGetMatterCount;

	[SaveField(null, null)]
	public DeckPreset deckPreset;

	[SaveField(null, null)]
	public List<PVPDefenderBattleRecord> battleRecords;

	[SaveField(null, null)]
	public DateTime creationTime;

	public DateTime ExpireTime => creationTime + TimeSpan.FromDays(7.0);

	public int SumBattleCount => battleRecords.Count;

	public int WinBattleCount => battleRecords.Count((PVPDefenderBattleRecord r) => r.challengerFinishType == FinishBattleEncounterAction.FinishType.Lose);

	public int SumDamageDealt => battleRecords.Sum((PVPDefenderBattleRecord r) => r.dealtDamageToChallenger);

	public float DamagePerBattle
	{
		get
		{
			if (SumBattleCount <= 0)
			{
				return 0f;
			}
			return (float)SumDamageDealt / (float)SumBattleCount;
		}
	}

	public int RandkScore => (int)(DamagePerBattle * 100f);

	public int SumGetMatterCount => WinBattleCount * ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerDefenderBattleWin + SumDamageDealt * ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerDefenderDamage;

	public MatterInfo RemainMatterToGet => new MatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter, remainGetMatterCount);

	public PVPEnemyAvatarConfig GetEnemyAvatarConfig(AssetCatalog assetCatalog)
	{
		return enemyAvatarRef.GetAssetPreData(assetCatalog).pvpEnemyAvatarConfig;
	}

	public PVPDefender()
	{
		guid = Guid.NewGuid().ToString();
		deckPreset = new DeckPreset();
		battleRecords = new List<PVPDefenderBattleRecord>();
		creationTime = DateTime.UtcNow;
	}

	public string GetBriefStateInfo(ApplicationStateInfo applicationStateInfo)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat("" + ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_sum_fight_count_arg0.FormatWith(SumBattleCount) + Environment.NewLine, ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_sum_dealt_damage_arg0.FormatWith(SumDamageDealt), Environment.NewLine), ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_damage_per_fight_arg0.FormatWith(DamagePerBattle.ToString("F")), Environment.NewLine), ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_sum_get_matter_arg0_count_arg1.FormatWith(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter.DisplayMatterName, SumGetMatterCount), Environment.NewLine), ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_defender_remove_remain_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(ExpireTime - DateTime.UtcNow)));
	}
}
