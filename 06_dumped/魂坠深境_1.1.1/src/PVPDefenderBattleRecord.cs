using System;
using System.Collections.Generic;
using UnityEngine;

public class PVPDefenderBattleRecord
{
	[SaveField(null, null)]
	public string defenderPresetGuid;

	[SaveField(null, null)]
	public string gameRecordId;

	[SaveField(null, null)]
	public int dealtDamageToChallenger;

	[SaveField(null, null)]
	public FinishBattleEncounterAction.FinishType challengerFinishType;

	[SaveField(null, null)]
	public string challengePlayerId;

	[SaveField(null, null)]
	public string challengePlayerNickName;

	[SaveField(null, null)]
	public string defenderPlayerId;

	[SaveField(null, null)]
	public string defenderName;

	[SaveField(null, null)]
	public string defenderPlayerNickName;

	[SaveField(null, null)]
	public string defenderAvatarId;

	[SaveField(null, null)]
	public UnityDateTime battleDateTime;

	public PVPDefenderBattleRecord()
	{
	}

	public PVPDefenderBattleRecord(BattleStateInfo battleState, FinishBattleEncounterAction.FinishType finishType)
	{
		PVPDefender pvpDefender = battleState.gameState.currentEncounter.setupInfo.pvpDefender;
		defenderPresetGuid = pvpDefender.guid;
		gameRecordId = battleState.gameState.GameGUID;
		dealtDamageToChallenger = Mathf.Clamp(battleState.gameState.recordManager.battleInitPlayerHealth - (int)battleState.player.health, 0, int.MaxValue);
		challengerFinishType = finishType;
		challengePlayerId = Bootstrap.instance.CurrentAccountID;
		challengePlayerNickName = battleState.gameState.applicationState.playerInfoManager.DataName;
		defenderPlayerId = pvpDefender.sourcePlayerId.ToString();
		defenderName = pvpDefender.defenderName;
		defenderPlayerNickName = pvpDefender.sourcePlayerName;
		defenderAvatarId = pvpDefender.enemyAvatarRef.CTAssetId;
		battleDateTime = new UnityDateTime(DateTime.UtcNow);
	}

	public List<MatterInfo> GetRewardMatterRewards(ApplicationStateInfo asi)
	{
		List<MatterInfo> list = new List<MatterInfo>();
		int num = 0;
		num += dealtDamageToChallenger * ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerDefenderDamage;
		if (challengerFinishType == FinishBattleEncounterAction.FinishType.Lose)
		{
			num += ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerDefenderBattleWin;
		}
		list.Add(new MatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter, num));
		return list;
	}
}
