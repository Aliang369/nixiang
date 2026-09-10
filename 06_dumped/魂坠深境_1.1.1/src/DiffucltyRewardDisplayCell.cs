using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiffucltyRewardDisplayCell : CellUI<(DifficultyInfo, ContractDef)>
{
	public Image difficultyIcon;

	public TextMeshProUGUI descriptionText;

	public GlobalItemDisplay globalItemDisplayPrefab;

	public RectTransform rewardDisplayParent;

	[NonSerialized]
	public CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardDisplayPool;

	protected override void Init()
	{
		rewardDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(globalItemDisplayPrefab, rewardDisplayParent);
	}

	protected override void OnSetup((DifficultyInfo, ContractDef) setupData)
	{
		var (difficultyInfo, contractDef) = setupData;
		difficultyIcon.sprite = difficultyInfo.icon;
		if (CellUI<(DifficultyInfo, ContractDef)>.applicationState.contractManager.GetContractState(contractDef).finishedMaxDifficultyThisCycle >= difficultyInfo.difficultyLevel)
		{
			descriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_finish_difficulty_arg0_reward_got_already.FormatWith(difficultyInfo.difficultyName);
			descriptionText.color = Color.grey;
		}
		else
		{
			descriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_finish_difficulty_arg0_to_get_reward.FormatWith(difficultyInfo.difficultyName);
			descriptionText.color = Color.white;
		}
		List<GlobalRewardSetup> difficultyReward = contractDef.GetDifficultyReward(difficultyInfo);
		rewardDisplayPool.DespawnAll();
		foreach (GlobalRewardSetup item in difficultyReward)
		{
			rewardDisplayPool.Spawn(item);
		}
	}
}
