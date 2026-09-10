using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public class AchievementManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<Achievement> achievements;

	[SaveField(null, null)]
	public List<AchievementDef> batchedUploadFinishedAchievements;

	public List<Achievement> globalAchievements => achievements.FindAll((Achievement ac) => ac.Type == AchievementDef.AchievementType.GlobalAchievement);

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationState.assetCatalog.achievementDefDefCatalog.ValidAssets.BuildToList(ref achievements, (AchievementDef acd, Achievement ac) => ac.achievementDef == acd, (AchievementDef acd) => new Achievement(acd));
		achievements.RemoveAll((Achievement a) => a.achievementDef == null);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeGameStart, _OnGameStart);
		if (batchedUploadFinishedAchievements == null)
		{
			batchedUploadFinishedAchievements = new List<AchievementDef>();
		}
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeGameStart, _OnGameStart);
	}

	public void InitAllAchievementObserve()
	{
		foreach (Achievement achievement in achievements)
		{
			achievement.Init(base.applicationState);
		}
	}

	public void ApplyProfileAchievements(IEnumerable<Achievement> profileAchievements, bool preserveSavedGameAchievements)
	{
		HashSet<AchievementDef> hashSet = null;
		if (preserveSavedGameAchievements)
		{
			hashSet = (from achievement in achievements.OrEmptyIfNull()
				where achievement != null && achievement.achievedInThisGame && (object)achievement.achievementDef != null
				select achievement.achievementDef).ToHashSet();
		}
		achievements = profileAchievements?.ToList() ?? new List<Achievement>();
		if (hashSet == null || hashSet.Count == 0)
		{
			return;
		}
		foreach (Achievement achievement in achievements)
		{
			if (achievement?.achievementDef != null && hashSet.Contains(achievement.achievementDef))
			{
				achievement.achievedInThisGame = true;
			}
		}
	}

	public void Achieve(Achievement achievement)
	{
		GameStateInfo runningGameState = base.applicationState.runningGameState;
		if (runningGameState != null && runningGameState.IsCharacterTrialGame)
		{
			return;
		}
		bool flag = false;
		if (GlobalConfigManager.instance.enableNetwork && !achievement.achieved)
		{
			flag = achievement.achievementDef.uploadImmediatelyOnAchieve;
			if (!flag && !batchedUploadFinishedAchievements.Contains(achievement.achievementDef))
			{
				batchedUploadFinishedAchievements.Add(achievement.achievementDef);
			}
		}
		if (!GlobalConfigManager.instance.enableNetwork && !achievement.achieved && string.IsNullOrWhiteSpace(achievement.finishedLocalDate))
		{
			achievement.finishedLocalDate = DateTime.Now.ToString("yyyy-MM-dd");
		}
		achievement.achieved = true;
		achievement.achievedInThisGame = base.applicationState.runningGameState != null;
		base.applicationState.savingManager.SetProfileDirtyToSave();
		base.applicationState.eventManager.achievementAchieveEvent.TriggerEvent(achievement);
		if (flag)
		{
			UploadAchievements(new List<AchievementDef> { achievement.achievementDef });
		}
	}

	public void UploadBatchedFinishedAchievements()
	{
		if (batchedUploadFinishedAchievements.IsNullOrEmpty())
		{
			return;
		}
		List<AchievementDef> finishedAchievements = new List<AchievementDef>(batchedUploadFinishedAchievements);
		UploadAchievements(finishedAchievements, delegate
		{
			foreach (AchievementDef item in finishedAchievements)
			{
				batchedUploadFinishedAchievements.Remove(item);
			}
		});
	}

	public void EnsureAchievementsUploaded(IEnumerable<AchievementDef> achievementDefs, Action onComplete)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			onComplete?.Invoke();
			return;
		}
		List<AchievementDef> pendingAchievements = achievementDefs?.Where((AchievementDef a) => a != null && batchedUploadFinishedAchievements.Contains(a)).Distinct().ToList();
		if (pendingAchievements.IsNullOrEmpty())
		{
			onComplete?.Invoke();
			return;
		}
		UploadAchievements(pendingAchievements, delegate
		{
			foreach (AchievementDef item in pendingAchievements)
			{
				batchedUploadFinishedAchievements.Remove(item);
			}
			onComplete?.Invoke();
		});
	}

	private void UploadAchievements(List<AchievementDef> achievementDefs, Action onComplete = null)
	{
		achievementDefs?.RemoveAll((AchievementDef a) => a == null);
		if (!achievementDefs.IsNullOrEmpty())
		{
			achievementDefs = achievementDefs.Distinct().ToList();
			GeneralGameAsyncOperationRequestOnly<TriggerAchievementRequest> generalGameAsyncOperationRequestOnly = new GeneralGameAsyncOperationRequestOnly<TriggerAchievementRequest>(new TriggerAchievementRequest(achievementDefs), base.applicationState.ChillyRoomSDK.Progress.TriggerAchievementAsync);
			if (onComplete != null)
			{
				generalGameAsyncOperationRequestOnly.OnComplete(onComplete);
			}
			generalGameAsyncOperationRequestOnly.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_uploading_achievement);
		}
	}

	public List<Achievement> GetAllGameAchivedAchievements()
	{
		List<Achievement> list = new List<Achievement>();
		foreach (Achievement achievement in achievements)
		{
			if (achievement.achievedInThisGame)
			{
				list.Add(achievement);
			}
		}
		return list;
	}

	public List<Achievement> GetAllAchievedAchievements()
	{
		List<Achievement> list = new List<Achievement>();
		foreach (Achievement achievement in achievements)
		{
			if (achievement.rewardReceived)
			{
				list.Add(achievement);
			}
		}
		return list;
	}

	private void _OnGameStart(GameEvent.Type type, GameEvent eve)
	{
		foreach (Achievement achievement in achievements)
		{
			achievement.achievedInThisGame = false;
		}
	}
}
