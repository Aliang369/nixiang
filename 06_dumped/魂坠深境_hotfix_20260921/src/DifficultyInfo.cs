using System.Collections.Generic;
using UnityEngine;

public class DifficultyInfo : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber
{
	[CTSOAssetName]
	[LocalizeText("难度名称")]
	public string difficultyName;

	[TextArea]
	[LocalizeText("难度描述")]
	public string description;

	public Sprite icon;

	public DifficultyLevel difficultyLevel;

	public ClavisDef diffucltyClavis;

	public List<ContractDef.ContractType> contractTypes;

	public List<Ability> ability;

	public int bonusExtraExp;

	public int expPerProgress;

	public List<GlobalRewardSetup> baseContractAccountFinishRewards;

	public List<GlobalRewardSetup> baseContractFinishRewards;

	public List<GlobalRewardSetup> hardChallengeContractFinishRewards;

	public List<GlobalRewardSetup> weekShuffleContractFinishRewards;

	public override string CTAssetIDPrepend => "DIFF";

	public string GetFullDetailDescription(ApplicationStateInfo asi)
	{
		return description.ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.difficultyDescriptionHighlightLineColor);
	}

	public List<GlobalRewardSetup> GetFirstFinishReward(ContractDef.ContractType type)
	{
		return type switch
		{
			ContractDef.ContractType.BaseGame => GetBaseGameClassFirstFinishReward(), 
			ContractDef.ContractType.Challenge => hardChallengeContractFinishRewards, 
			ContractDef.ContractType.HardChallenge => hardChallengeContractFinishRewards, 
			ContractDef.ContractType.WeekShuffle => weekShuffleContractFinishRewards, 
			_ => new List<GlobalRewardSetup>(), 
		};
	}

	public List<GlobalRewardSetup> GetBaseGameAccountFirstFinishReward()
	{
		return baseContractAccountFinishRewards;
	}

	public List<GlobalRewardSetup> GetBaseGameClassFirstFinishReward()
	{
		return baseContractFinishRewards;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public new List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (Ability item in ability.OrEmptyIfNull())
		{
			list.Add(item);
		}
		return list;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in ability.OrEmptyIfNull())
		{
			if (item == null)
			{
				continue;
			}
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		foreach (Ability item in ability.OrEmptyIfNull())
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
		foreach (Ability item in ability.OrEmptyIfNull())
		{
			item?.TriggerGameEvent(eve);
		}
	}

	public static DifficultyInfo GetDifficultyInfo(DifficultyLevel level, AssetCatalog catalog)
	{
		return catalog.difficultyCatalog.ValidAssets.Find((DifficultyInfo d) => d.difficultyLevel == level);
	}
}
