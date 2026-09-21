using System;
using System.Collections.Generic;

public sealed class YuanQiActivityEventHandler : IDisposable
{
	private readonly ApplicationStateInfo applicationState;

	private readonly YuanQiActivityModel model;

	private readonly Func<bool> canCollect;

	private readonly Action onMissionChanged;

	private readonly Action<YuanQiActivityMissionItem, string> onMissionReached;

	public YuanQiActivityEventHandler(ApplicationStateInfo applicationState, YuanQiActivityModel model, Func<bool> canCollect, Action onMissionChanged, Action<YuanQiActivityMissionItem, string> onMissionReached)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
		this.model = model ?? throw new ArgumentNullException("model");
		this.canCollect = canCollect;
		this.onMissionChanged = onMissionChanged;
		this.onMissionReached = onMissionReached;
		applicationState.eventManager.activityMissionGiftCodeEvent.AddListener(OnGiftCode);
		applicationState.eventManager.activityMissionShareEvent.AddListener(OnShare);
		applicationState.eventManager.afterGameOverEvent.AddListener(OnAfterGameOver);
	}

	public void Dispose()
	{
		applicationState.eventManager?.activityMissionGiftCodeEvent.RemoveListener(OnGiftCode);
		applicationState.eventManager?.activityMissionShareEvent.RemoveListener(OnShare);
		applicationState.eventManager?.afterGameOverEvent.RemoveListener(OnAfterGameOver);
	}

	private void OnGiftCode(YuanQiActivityMissionGiftCodeEventData data)
	{
		Collect(YuanQiActivityMissionType.GiftCode, (YuanQiActivityMissionData _) => true, null, $"code={data.code}, rewards={data.rewards?.Count ?? 0}");
		RepushReachedMissions(YuanQiActivityMissionType.GiftCode);
	}

	private void RepushReachedMissions(YuanQiActivityMissionType type)
	{
		foreach (YuanQiActivityMissionItem missionItem in model.MissionItems)
		{
			if (missionItem?.Data != null && missionItem.Data.type == type && missionItem.Status == YuanQiActivityMissionStatus.Reached)
			{
				onMissionReached?.Invoke(missionItem, null);
			}
		}
	}

	private void OnShare(YuanQiActivityMissionShareEventData data)
	{
		Collect(YuanQiActivityMissionType.Share, (YuanQiActivityMissionData config) => IsCharacterMatched(config, data.character), data.record?.gameGuid, "source=" + data.source + ", character=" + (data.character?.characterName ?? "null") + ", record=" + (data.record?.gameGuid ?? "null"));
	}

	private void OnAfterGameOver(GameRecord record)
	{
		Collect(YuanQiActivityMissionType.BossKill, (YuanQiActivityMissionData config) => IsBossKillMissionAchieved(config, record), record?.gameGuid, string.Format("record={0}, success={1}, stage={2}, difficulty={3}, character={4}", record?.gameGuid ?? "null", record?.success, record?.stage, record?.contract?.difficultyLevel, record?.character?.characterName ?? "null"));
	}

	private void Collect(YuanQiActivityMissionType type, Func<YuanQiActivityMissionData, bool> predicate, string gameGuid, string detail)
	{
		Func<bool> func = canCollect;
		if (func != null && !func())
		{
			return;
		}
		IReadOnlyList<YuanQiActivityMissionItem> readOnlyList = model.CollectMissions(type, predicate);
		if (readOnlyList.Count == 0)
		{
			return;
		}
		foreach (YuanQiActivityMissionItem item in readOnlyList)
		{
			TraceLog.Debug($"[YuanQiActivityEventHandler] Collect {item.Data.missionId}, progress={item.Progress}/{item.Data.targetProgress}, status={item.Status}, {detail}");
			if (item.Status == YuanQiActivityMissionStatus.Reached)
			{
				onMissionReached?.Invoke(item, gameGuid);
			}
		}
		onMissionChanged?.Invoke();
	}

	private static bool IsBossKillMissionAchieved(YuanQiActivityMissionData data, GameRecord record)
	{
		TraceLog.Debug(record.character.FullID);
		if (record == null || !record.success || record.contract == null)
		{
			return false;
		}
		if (!IsCharacterMatched(data, record.character))
		{
			return false;
		}
		if (data.minDifficulty.HasValue && record.contract.difficultyLevel < data.minDifficulty.Value)
		{
			return false;
		}
		if (data.stage.HasValue && record.stage != data.stage.Value)
		{
			return false;
		}
		return true;
	}

	private static bool IsCharacterMatched(YuanQiActivityMissionData data, PlayerCharacterDef character)
	{
		if (string.IsNullOrEmpty(data.characterId))
		{
			return true;
		}
		if (character != null)
		{
			return character.FullID == data.characterId;
		}
		return false;
	}
}
