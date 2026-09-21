using System.Collections.Generic;

public class Achievement : IGameEventSubscriber, IValidate
{
	[SaveField(null, null)]
	public bool achieved;

	[SaveField(null, null)]
	public bool achievedInThisGame;

	[SaveField(null, null)]
	public AchievementDef achievementDef;

	[SaveField(null, null)]
	public bool rewardReceived;

	[SaveField(null, null)]
	public string finishedLocalDate;

	public AchievementDef.AchievementType Type => achievementDef.type;

	public AchievementDef.AchievementLayer Layer => achievementDef.layer;

	public bool GetRewardAble
	{
		get
		{
			if (achieved)
			{
				return !rewardReceived;
			}
			return false;
		}
	}

	private Achievement()
	{
	}

	public Achievement(AchievementDef achievementDef)
	{
		this.achievementDef = achievementDef;
		achieved = false;
		rewardReceived = false;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		new HashSet<GameEvent.Type>();
		if (achievedInThisGame)
		{
			yield break;
		}
		foreach (Ability item in achievementDef.abilities.OrEmptyIfNull())
		{
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
		if (achievementDef.globalTriggerObserver is GameEventObserver gameEventObserver)
		{
			yield return gameEventObserver.type;
		}
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		foreach (Ability item in achievementDef.abilities.OrEmptyIfNull())
		{
			item.TriggerGameEvent(eve);
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (achievedInThisGame)
		{
			return false;
		}
		if (eve != null && eve.gameState?.IsCharacterTrialGame == true)
		{
			return false;
		}
		foreach (Ability item in achievementDef.abilities.OrEmptyIfNull())
		{
			if (item.CheckTriggerGameEventAble(eve))
			{
				return true;
			}
		}
		return false;
	}

	public void Init(ApplicationStateInfo applicationState)
	{
		if (achievementDef?.globalTriggerObserver != null)
		{
			achievementDef.globalTriggerObserver.Init(applicationState, OnAchievementObserverTrigger);
		}
		void OnAchievementObserverTrigger()
		{
			if (applicationState.achievementManager.achievements.Contains(this) && (achievementDef.type != AchievementDef.AchievementType.GameAchievement || !achievedInThisGame) && (achievementDef.type != AchievementDef.AchievementType.GlobalAchievement || !achieved) && (applicationState.runningGameState == null || !applicationState.runningGameState.IsIntroGame || achievementDef.triggerableInIntro) && (applicationState.runningGameState == null || !applicationState.runningGameState.IsCharacterTrialGame) && MatchAttachCharacterClass(applicationState))
			{
				applicationState.achievementManager.Achieve(this);
			}
		}
	}

	private bool MatchAttachCharacterClass(ApplicationStateInfo applicationState)
	{
		if (achievementDef?.attachCharacterClass == null)
		{
			return true;
		}
		if (applicationState?.runningGameState == null)
		{
			return true;
		}
		return applicationState?.runningGameState?.playerState?.CharacterDef?.attachClass == achievementDef.attachCharacterClass;
	}

	public GlobalRewardPackage GetReward(ApplicationStateInfo applicationState)
	{
		if (rewardReceived)
		{
			return null;
		}
		rewardReceived = true;
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (GlobalRewardSetup achieveReward in achievementDef.AchieveRewards)
		{
			list.Add(achieveReward.GenerateReward(applicationState, GlobalRewardSourceType.AchievementReward, achievementDef));
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.GiveAll(applicationState);
		globalRewardPackage.title = achievementDef.achievementName;
		return globalRewardPackage;
	}

	public string GetDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return achievementDef.GetDescription(applicationState, out generator);
	}

	public bool IsValidate()
	{
		return achievementDef != null;
	}
}
