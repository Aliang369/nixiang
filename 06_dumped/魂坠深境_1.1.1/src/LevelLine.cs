using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using GameRandom;
using UnityEngine;

public class LevelLine
{
	public class ChangeExpResult
	{
		public class LevelRewardInfo
		{
			[SaveField(null, null)]
			public int levelIndex;

			[SaveField(null, null)]
			public GlobalRewardInfo baseReward;

			[SaveField(null, null)]
			public GlobalRewardInfo appendReward;
		}

		[SaveField(null, null)]
		public ChangeExpRequest request;

		[SaveField(null, null)]
		public LevelLineDef levelLineDef;

		[SaveField(null, null)]
		public int initExp;

		[SaveField(null, null)]
		public int initLeveLIndex;

		[SaveField(null, null)]
		public int finalExp;

		[SaveField(null, null)]
		public int finalLevelIndex;

		[SaveField(null, null)]
		public List<LevelRewardInfo> levelUps;

		[SaveField(null, null)]
		public List<GlobalRewardInfo> dumpRewards;

		[SaveField(null, null)]
		public bool fromQuest;

		public int deltaExp
		{
			get
			{
				if (initLeveLIndex == finalLevelIndex)
				{
					return finalExp - initExp;
				}
				int num = levelLineDef.levels[initLeveLIndex].expCost - initExp;
				for (int i = initLeveLIndex + 1; i < finalLevelIndex; i++)
				{
					num += levelLineDef.levels[i].expCost;
				}
				return num + finalExp;
			}
		}

		public List<GlobalRewardInfo> GetAllRewards()
		{
			List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
			if (levelUps == null)
			{
				return list;
			}
			foreach (LevelRewardInfo levelUp in levelUps)
			{
				if (levelUp.baseReward != null)
				{
					list.Add(levelUp.baseReward);
				}
				if (levelUp.appendReward != null)
				{
					list.Add(levelUp.appendReward);
				}
			}
			if (dumpRewards != null)
			{
				foreach (GlobalRewardInfo dumpReward in dumpRewards)
				{
					list.Add(dumpReward);
				}
			}
			return list;
		}

		public List<GlobalRewardInfo> GetAllUnFoldRewards()
		{
			return GlobalRewardInfo.UnfoldNestedReward(GetAllRewards());
		}

		public LevelLine GenerateBeforeLevelLine()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < initLeveLIndex; i++)
			{
				list.Add(i);
			}
			return new LevelLine(levelLineDef)
			{
				currentExp = initExp,
				currentLevelIndex = initLeveLIndex,
				levelLineDef = levelLineDef,
				baseRewardGotLevels = list
			};
		}

		public LevelLine GenerateAfterLevelLine()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < finalLevelIndex; i++)
			{
				list.Add(i);
			}
			return new LevelLine(levelLineDef)
			{
				currentExp = finalExp,
				currentLevelIndex = finalLevelIndex,
				levelLineDef = levelLineDef,
				baseRewardGotLevels = list
			};
		}

		public void Excute(ApplicationStateInfo applicationState)
		{
			if (levelLineDef == null)
			{
				return;
			}
			LevelLine levelLineOfDef = levelLineDef.GetLevelLineOfDef(applicationState);
			levelLineOfDef.currentExp = finalExp;
			levelLineOfDef.currentLevelIndex = finalLevelIndex;
			foreach (LevelRewardInfo levelUp in levelUps)
			{
				if (levelUp.baseReward != null)
				{
					levelUp.baseReward.Give(applicationState);
					levelLineOfDef.baseRewardGotLevels.AddUnique(levelUp.levelIndex);
				}
				if (levelUp.appendReward != null)
				{
					levelUp.appendReward.Give(applicationState);
					levelLineOfDef.appendRewardGotLevels.AddUnique(levelUp.levelIndex);
				}
			}
			foreach (GlobalRewardInfo dumpReward in dumpRewards)
			{
				dumpReward.Give(applicationState);
				levelLineOfDef.dumpExtraLevelCount++;
			}
			if (fromQuest)
			{
				applicationState.seasonManager.gotQuestExpThisWeek += request.deltaExp;
			}
			applicationState.eventManager.changeExpResultExecuteEvent.TriggerEvent(this);
			ThinkingAnalyticsExtends.TrackLevelLineChangeExp(applicationState, this);
			applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public class ChangeExpRequest
	{
		[SaveField(null, null)]
		public LevelLineDef.LevelLineType type;

		[SaveField(null, null)]
		public int deltaExp;

		[SaveField(null, null)]
		public bool fromQuest;

		public static ChangeExpRequest ChangeLevelLineExp(int exp, LevelLine line, ApplicationStateInfo applicationState)
		{
			if (line == null)
			{
				return null;
			}
			if (line.levelLineDef == null)
			{
				return null;
			}
			if (line.levelLineDef.type == LevelLineDef.LevelLineType.Global)
			{
				return ChangeGlobalExp(exp);
			}
			if (line.levelLineDef.type == LevelLineDef.LevelLineType.Season)
			{
				return ChangeSeasonExp(exp, fromQuest: false, applicationState);
			}
			return null;
		}

		public static ChangeExpRequest ChangeGlobalExp(int exp)
		{
			return new ChangeExpRequest
			{
				type = LevelLineDef.LevelLineType.Global,
				deltaExp = exp
			};
		}

		public static ChangeExpRequest ChangeSeasonExp(int exp, bool fromQuest, ApplicationStateInfo applicationState)
		{
			if (fromQuest)
			{
				int b = applicationState.seasonManager.FinalWeeklyQuestSeasonExpLimit - applicationState.seasonManager.gotQuestExpThisWeek;
				exp = Mathf.Min(exp, b);
			}
			return new ChangeExpRequest
			{
				type = LevelLineDef.LevelLineType.Season,
				deltaExp = exp,
				fromQuest = fromQuest
			};
		}

		public LevelLine GetCurrentLevelLine(ApplicationStateInfo applicationState)
		{
			if (type == LevelLineDef.LevelLineType.Global)
			{
				return applicationState.globalUnlockManager.globalLevelLine;
			}
			if (type == LevelLineDef.LevelLineType.Season)
			{
				return applicationState.seasonManager.currentSeason.seasonLeveLine;
			}
			return null;
		}

		public ChangeExpResult Generate(ApplicationStateInfo applicationState, string title = "", string hint = "")
		{
			LevelLine currentLevelLine = GetCurrentLevelLine(applicationState);
			if (currentLevelLine == null)
			{
				return null;
			}
			int currentExp = currentLevelLine.currentExp;
			int currentLevelIndex = currentLevelLine.currentLevelIndex;
			int num = currentExp;
			int num2 = currentLevelIndex;
			List<ChangeExpResult.LevelRewardInfo> list = new List<ChangeExpResult.LevelRewardInfo>();
			List<GlobalRewardInfo> list2 = new List<GlobalRewardInfo>();
			int num3 = deltaExp;
			while (num3 > 0 && num2 < currentLevelLine.MaxLevelIndex)
			{
				int num4 = currentLevelLine.GetLineLevel(num2).expCost - num;
				if (num4 > num3)
				{
					num += num3;
					num3 = 0;
					continue;
				}
				LevelLineDef.LineLevel lineLevel = currentLevelLine.GetLineLevel(num2);
				GlobalRewardInfo baseReward = (currentLevelLine.levelLineDef.levelLineAutoGiveReward ? lineLevel.GenerateBaseReward(applicationState, currentLevelLine) : null);
				GlobalRewardInfo appendReward = ((currentLevelLine.levelLineDef.levelLineAutoGiveReward && currentLevelLine.IsAppendRewardUnlocked(applicationState)) ? lineLevel.GenerateAppendReward(applicationState, currentLevelLine) : null);
				list.Add(new ChangeExpResult.LevelRewardInfo
				{
					levelIndex = num2,
					baseReward = baseReward,
					appendReward = appendReward
				});
				num = 0;
				num2++;
				num3 -= num4;
			}
			if (num2 == currentLevelLine.MaxLevelIndex && num3 > 0 && currentLevelLine.levelLineDef.expDump != null)
			{
				while (num3 > 0)
				{
					if (num + num3 >= currentLevelLine.levelLineDef.expDump.expCost)
					{
						num3 -= currentLevelLine.levelLineDef.expDump.expCost - num;
						num = 0;
						list2.Add(currentLevelLine.next.GenerateReward(applicationState, GlobalRewardSourceType.LevelLineDumpReward, currentLevelLine.levelLineDef));
						currentLevelLine.next = currentLevelLine.levelLineDef.expDump.rewardSetups.SelectRandom();
					}
					else
					{
						num = num3;
						num3 = 0;
					}
				}
			}
			return new ChangeExpResult
			{
				request = this,
				levelLineDef = currentLevelLine.levelLineDef,
				initExp = currentExp,
				initLeveLIndex = currentLevelIndex,
				finalExp = num,
				finalLevelIndex = num2,
				dumpRewards = list2,
				levelUps = list,
				fromQuest = fromQuest
			};
		}
	}

	[SaveField(null, null)]
	public LevelLineDef levelLineDef;

	[SaveField(null, null)]
	public GlobalRewardSetup next;

	[SaveField(null, null)]
	public int currentExp;

	[SaveField(null, null)]
	public int currentLevelIndex;

	[SaveField(null, null)]
	public List<int> baseRewardGotLevels;

	[SaveField(null, null)]
	public List<int> appendRewardGotLevels;

	[SaveField(null, null)]
	public int dumpExtraLevelCount;

	public int MaxLevelIndex => levelLineDef.levels.Count;

	public int CurrentDisplayLevel => currentLevelIndex + 1;

	public bool IsMaxLevel => currentLevelIndex >= MaxLevelIndex;

	public LevelLineDef.LineLevel CurrentLevel => GetLineLevel(currentLevelIndex);

	public LevelLineDef.LineLevel LastLevel => GetLineLevel(levelLineDef.levels.Count - 1);

	public int CurrentLevelExpCost => CurrentLevel?.expCost ?? int.MaxValue;

	public int SumGotExp => levelLineDef.levels.Take(currentLevelIndex).Sum((LevelLineDef.LineLevel l) => l.expCost) + currentExp;

	private LevelLine()
	{
	}

	public LevelLine(LevelLineDef levelLineDef)
	{
		this.levelLineDef = levelLineDef;
		currentExp = 0;
		currentLevelIndex = 0;
		baseRewardGotLevels = new List<int>();
		appendRewardGotLevels = new List<int>();
		if (levelLineDef.expDump?.rewardSetups != null)
		{
			next = levelLineDef.expDump.rewardSetups.SelectRandom();
		}
	}

	public LevelLineDef.LineLevel GetLineLevel(int index)
	{
		return levelLineDef.levels.IndexOrNull(index);
	}

	public int GetLineLevelIndex(LevelLineDef.LineLevel level)
	{
		return levelLineDef.GetLineLevelIndex(level);
	}

	public List<GlobalRewardSetup> GetAllUnGotRewardInLevelRange(ApplicationStateInfo applicationStateInfo)
	{
		return GetUnGotRewardInLevelRange(applicationStateInfo, 0, currentLevelIndex);
	}

	public List<GlobalRewardSetup> GetUnGotRewardInLevelRange(ApplicationStateInfo applicationStateInfo, int fromLevelIndex, int toLevelIndex)
	{
		List<GlobalRewardSetup> list = new List<GlobalRewardSetup>();
		for (int i = fromLevelIndex; i < toLevelIndex; i++)
		{
			if (i >= 0 && i < levelLineDef.levels.Count)
			{
				LevelLineDef.LineLevel lineLevel = GetLineLevel(i);
				if (!baseRewardGotLevels.Contains(i) && lineLevel.reward != null)
				{
					list.Add(lineLevel.reward);
				}
				if (!appendRewardGotLevels.Contains(i) && applicationStateInfo.seasonManager.currentSeason.seasonPassUnlocked && lineLevel.appendReward != null)
				{
					list.Add(lineLevel.appendReward);
				}
			}
		}
		return list;
	}

	public List<GlobalRewardSetup> GetAllAppendRewards()
	{
		List<GlobalRewardSetup> list = new List<GlobalRewardSetup>();
		for (int i = 0; i < levelLineDef.levels.Count; i++)
		{
			LevelLineDef.LineLevel lineLevel = levelLineDef.levels[i];
			if (lineLevel.appendReward != null)
			{
				list.Add(lineLevel.appendReward);
			}
		}
		return list;
	}

	public bool WithAnyUngotReward(ApplicationStateInfo applicationStateInfo)
	{
		for (int i = 0; i < levelLineDef.levels.Count && i < currentLevelIndex; i++)
		{
			_ = levelLineDef.levels[i];
			if (!baseRewardGotLevels.Contains(i))
			{
				return true;
			}
			if (!appendRewardGotLevels.Contains(i) && applicationStateInfo.seasonManager.currentSeason.seasonPassUnlocked)
			{
				return true;
			}
		}
		return false;
	}

	public void GenerateAndGiveAllUnGotLevelLineReward(ApplicationStateInfo applicationStateInfo, Action<List<ChangeExpResult.LevelRewardInfo>> callback)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		GetUnGotLevelLineRewardRequest request = new GetUnGotLevelLineRewardRequest(this);
		GeneralGameAsyncOperation<GetUnGotLevelLineRewardRequest, GetUnGotLevelLineRewardResult> op = new GeneralGameAsyncOperation<GetUnGotLevelLineRewardRequest, GetUnGotLevelLineRewardResult>(request, applicationStateInfo.ChillyRoomSDK.Season.GetUnGotLevelLineRewardAsync);
		op.OnComplete(delegate
		{
			NetworkConvertContext context = new NetworkConvertContext(applicationStateInfo);
			List<ChangeExpResult.LevelRewardInfo> levelUps = op.OpResult.LevelLineRewards.Select((LevelUpRewardInfo r) => r.ToClient(context)).ToList();
			GiveAfterGenerate(levelUps);
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_level_line);
		void GiveAfterGenerate(List<ChangeExpResult.LevelRewardInfo> levelUps)
		{
			foreach (ChangeExpResult.LevelRewardInfo levelUp in levelUps)
			{
				if (levelUp.baseReward != null)
				{
					baseRewardGotLevels.Add(levelUp.levelIndex);
					levelUp.baseReward.Give(applicationStateInfo);
				}
				if (levelUp.appendReward != null)
				{
					appendRewardGotLevels.Add(levelUp.levelIndex);
					levelUp.appendReward.Give(applicationStateInfo);
				}
			}
			callback?.Invoke(levelUps);
		}
	}

	public bool IsAppendRewardUnlocked(ApplicationStateInfo applicationState)
	{
		if (levelLineDef.type == LevelLineDef.LevelLineType.Season && !applicationState.seasonManager.currentSeason.seasonPassUnlocked)
		{
			return false;
		}
		return true;
	}

	public bool IsGlobalLevelLine(ApplicationStateInfo applicationState)
	{
		return applicationState.globalUnlockManager.globalLevelLine == this;
	}
}
