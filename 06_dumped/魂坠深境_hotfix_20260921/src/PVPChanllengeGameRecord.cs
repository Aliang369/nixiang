using System.Collections.Generic;
using System.Linq;

public class PVPChanllengeGameRecord
{
	[SaveField(null, null)]
	public string gameGuid;

	[SaveField(null, null)]
	public List<PVPDefenderBattleRecord> defenderBattleRecords;

	private PVPChanllengeGameRecord()
	{
	}

	public PVPChanllengeGameRecord(string gameGuid)
	{
		this.gameGuid = gameGuid;
		defenderBattleRecords = new List<PVPDefenderBattleRecord>();
	}

	public GlobalGameRecordManager.GameRecordPreviewInfo GetRecordPreviewInfo(ApplicationStateInfo asi)
	{
		return asi.recordManager.records.FirstOrDefault((GlobalGameRecordManager.GameRecordPreviewInfo r) => r.gameGuid == gameGuid);
	}

	public List<MatterInfo> GetRewardMatterRewards(ApplicationStateInfo asi)
	{
		bool success = GetRecordPreviewInfo(asi).success;
		int num = defenderBattleRecords.Count((PVPDefenderBattleRecord r) => r.challengerFinishType == FinishBattleEncounterAction.FinishType.Win);
		int num2 = 0;
		if (success)
		{
			num2 += ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusOnChallengeGameWin;
		}
		num2 += num * ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerChallengeBattleWin;
		return new List<MatterInfo>
		{
			new MatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter, num2)
		};
	}
}
