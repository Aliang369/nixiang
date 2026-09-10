using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RoutineQuest
{
	[SaveField(null, null)]
	public string guid;

	[SaveField(null, null)]
	public RoutineQuestDef def;

	[SaveField(null, null)]
	public int gotRewardLayer;

	[SaveField(null, null)]
	public int progress;

	[SaveField(null, null)]
	public PlayerCharacterDef character;

	[SaveField(null, null)]
	public SkillDef skill;

	public int sumExp => def.rewards.Sum((GlobalRewardSetup r) => (r as AddCurrentSeasonLevelExp)?.addExp ?? 0);

	public int CurrentMaxProgress => (Mathf.Min(gotRewardLayer + 1, def?.maxLayer ?? 0) * def?.maxProgress).GetValueOrDefault();

	public bool CurrentLayerFinished => progress >= CurrentMaxProgress;

	public bool AllLayerFinished => gotRewardLayer >= def?.maxLayer;

	public bool GetRewardAble
	{
		get
		{
			if (CurrentLayerFinished)
			{
				return !AllLayerFinished;
			}
			return false;
		}
	}

	public RoutineQuestDef.RoutineQuestNode.JumpPanelType JumpPanelType => def.node.JumpToPanelType;

	private RoutineQuest()
	{
	}

	public RoutineQuest(RoutineQuestDef def, ApplicationStateInfo applicationState)
	{
		this.def = def;
		guid = Guid.NewGuid().ToString();
		progress = 0;
		gotRewardLayer = 0;
		if (def.node.ExtraInfoType == RoutineQuestDef.QuestExtraInfoType.RandomOwnedCharacter)
		{
			character = applicationState.characterManager.GetAllUnlockedCharacterStates().ToList().SelectRandom()
				.def;
		}
		if (def.node.ExtraInfoType == RoutineQuestDef.QuestExtraInfoType.RandomOwnedSkill)
		{
			skill = applicationState.globalSkillManager.allSkills.ToList().SelectRandom().def;
		}
	}

	public GlobalRewardPackage GetQuestReward(ApplicationStateInfo applicationState)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		gotRewardLayer++;
		foreach (GlobalRewardSetup reward in def.rewards)
		{
			list.Add(reward.GenerateReward(applicationState, GlobalRewardSourceType.RoutineQuest, def));
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.GiveAll(applicationState);
		return globalRewardPackage;
	}

	public string GetDescription(ApplicationStateInfo applicationState)
	{
		string newValue = progress.ToString();
		string newValue2 = CurrentMaxProgress.ToString();
		string newValue3 = def.AllLayerSumProgress.ToString();
		string newValue4 = def.node.QuestTargetText(this);
		return def.description.Replace("<Progress>", newValue).Replace("<MaxProgress>", newValue2).Replace("<AllLayerMaxProgress>", newValue3)
			.Replace("<Target>", newValue4);
	}
}
