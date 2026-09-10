using System;
using System.Collections.Generic;
using System.Linq;

public class Contract : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber
{
	private readonly struct DifficultyProgressDelta(DifficultyLevel oldProgress, DifficultyLevel newProgress)
	{
		public DifficultyLevel OldProgress { get; } = oldProgress;

		public DifficultyLevel NewProgress { get; } = newProgress;

		public bool Advanced => NewProgress > OldProgress;
	}

	[SaveField(null, null)]
	public ContractDef contractDef;

	[SaveField(null, null)]
	public DifficultyLevel difficultyLevel;

	public string ContractName => contractDef.Name;

	public bool IsFullLongGame
	{
		get
		{
			if (contractDef.contractType != ContractDef.ContractType.BaseGame && contractDef.contractType != ContractDef.ContractType.Challenge)
			{
				return contractDef.contractType == ContractDef.ContractType.HardChallenge;
			}
			return true;
		}
	}

	public ContractState GetContractState(ApplicationStateInfo applicationState)
	{
		return applicationState.contractManager.GetContractState(contractDef);
	}

	public Level GetInitLevel(AssetCatalog catalog)
	{
		return Level.GetStageLevels(1, catalog).First();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return ((IChildren<IEffectNode>)contractDef).GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in contractDef.abilities.OrEmptyIfNull())
		{
			foreach (GameEvent.Type item2 in item?.GetAllSubscribeGameEventType().OrEmptyIfNull())
			{
				yield return item2;
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		foreach (Ability item in contractDef.abilities.OrEmptyIfNull())
		{
			if (item != null && item.CheckTriggerGameEventAble(eve))
			{
				return true;
			}
		}
		return false;
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability item in contractDef.abilities.OrEmptyIfNull())
		{
			item?.TriggerGameEvent(eve);
		}
	}

	public DifficultyInfo GetDifficultyInfo(ApplicationStateInfo applicationState)
	{
		return applicationState.difficultyManager.GetDifficultyInfo(difficultyLevel);
	}

	public string GetContractDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatContractDescription(this, applicationState, out generator);
	}

	public void FinishContract(ApplicationStateInfo applicationState, PlayerCharacterDef finishedCharacter = null, bool applyDifficultyProgression = true)
	{
		if (contractDef != null)
		{
			applicationState.eventManager.finishContractEvent.TriggerEvent(this);
		}
		ContractState contractState = applicationState.contractManager.GetContractState(contractDef);
		contractState.finishedThisCycle = true;
		if (applyDifficultyProgression)
		{
			DifficultyProgressDelta difficultyProgress = GetDifficultyProgress(contractState.finishedMaxDifficultyThisCycle);
			if (difficultyProgress.Advanced)
			{
				contractState.finishedMaxDifficultyThisCycle = difficultyProgress.NewProgress;
			}
		}
		if (!applyDifficultyProgression || !(contractDef != null) || contractDef.contractType != ContractDef.ContractType.BaseGame)
		{
			return;
		}
		DifficultyProgressDelta difficultyProgress2 = GetDifficultyProgress(applicationState.difficultyManager.finishedMaxDifficulty);
		if (difficultyProgress2.Advanced)
		{
			applicationState.difficultyManager.FinishDifficulty(difficultyProgress2.NewProgress);
		}
		CharacterClassDef characterClassDef = finishedCharacter?.attachClass ?? applicationState.characterManager.currentCharacter?.def?.attachClass;
		if (characterClassDef != null)
		{
			DifficultyProgressDelta difficultyProgress3 = GetDifficultyProgress(applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass));
			if (difficultyProgress3.Advanced)
			{
				applicationState.idolManager.AdvanceMainlineDifficulty(characterClassDef.mClass, difficultyProgress3.NewProgress);
			}
		}
	}

	public List<GlobalRewardInfo> GetSuccessReward(ApplicationStateInfo applicationState, PlayerCharacterDef finishedCharacter = null, bool includeDifficultyRewards = true)
	{
		ContractState contractState = GetContractState(applicationState);
		List<GlobalRewardInfo> rewards = new List<GlobalRewardInfo>();
		if (!contractState.finishedThisCycle)
		{
			foreach (GlobalRewardSetup finishReward in contractDef.finishRewards)
			{
				rewards.Add(finishReward.GenerateReward(applicationState, GlobalRewardSourceType.ContractFinishReward, contractDef));
			}
		}
		if (!includeDifficultyRewards)
		{
			return rewards;
		}
		if (contractDef != null && contractDef.contractType == ContractDef.ContractType.BaseGame)
		{
			AddDifficultyRewards(GetDifficultyProgress(applicationState.difficultyManager.finishedMaxDifficulty), contractDef.GetAccountFirstFinishRewardInDifficulty, GlobalRewardSourceType.ContractAccountDifficultyReward);
			CharacterClassDef characterClassDef = finishedCharacter?.attachClass ?? applicationState.characterManager.currentCharacter?.def?.attachClass;
			if (characterClassDef != null)
			{
				AddDifficultyRewards(GetDifficultyProgress(applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass)), contractDef.GetClassFirstFinishRewardInDifficulty, GlobalRewardSourceType.ContractClassDifficultyReward);
			}
		}
		else
		{
			AddDifficultyRewards(GetDifficultyProgress(contractState.finishedMaxDifficultyThisCycle), contractDef.GetFirstFinishRewardInDifficulty, GlobalRewardSourceType.ContractDifficultyReward);
		}
		return rewards;
		void AddDifficultyRewards(DifficultyProgressDelta progress, Func<ApplicationStateInfo, DifficultyLevel, List<GlobalRewardSetup>> rewardGetter, GlobalRewardSourceType sourceType)
		{
			if (!progress.Advanced)
			{
				return;
			}
			foreach (DifficultyInfo validAsset in applicationState.assetCatalog.difficultyCatalog.ValidAssets)
			{
				if (validAsset.difficultyLevel >= DifficultyLevel.Normal && validAsset.difficultyLevel > progress.OldProgress && validAsset.difficultyLevel <= progress.NewProgress)
				{
					foreach (GlobalRewardSetup item in rewardGetter(applicationState, validAsset.difficultyLevel).OrEmptyIfNull())
					{
						rewards.Add(item.GenerateReward(applicationState, sourceType, contractDef));
					}
				}
			}
		}
	}

	private DifficultyProgressDelta GetDifficultyProgress(DifficultyLevel oldProgress)
	{
		DifficultyLevel newProgress = ((difficultyLevel > oldProgress) ? difficultyLevel : oldProgress);
		return new DifficultyProgressDelta(oldProgress, newProgress);
	}

	public IEnumerable<DifficultyInfo> GetUnlockedDifficulties(ApplicationStateInfo applicationState)
	{
		foreach (DifficultyInfo validAsset in applicationState.assetCatalog.difficultyCatalog.ValidAssets)
		{
			if (validAsset.difficultyLevel >= DifficultyLevel.Normal && validAsset.difficultyLevel <= difficultyLevel)
			{
				yield return validAsset;
			}
		}
	}
}
