using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class GameLootRewardsDisplayArea : UIBase
{
	public GameLootRewardCell prefab;

	public RectTransform contentArea;

	public Sprite gemIcon;

	public GameObject contentDescriptionGameObject;

	public TextMeshProUGUI contentDescriptionText;

	[NonSerialized]
	private CellUIPrefabPool<GameLootRewardCell, GameLootRewardDisplayData> pool;

	protected override void Init()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		pool = GetPrefabPool<GameLootRewardCell, GameLootRewardDisplayData>(prefab, contentArea);
		if (contentDescriptionGameObject != null)
		{
			contentDescriptionGameObject.SetActive(value: false);
		}
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		List<GameLootRewardDisplayData> list = GenerateAllRewardsDisplayData();
		contentArea.gameObject.SetActive(list.Count > 0);
		pool.Sync(list);
		contentDescriptionGameObject.SetActive(value: false);
	}

	public void ShowInfo(string description)
	{
		contentDescriptionGameObject.SetActive(value: true);
		contentDescriptionText.text = description;
	}

	private List<GameLootRewardDisplayData> GenerateAllRewardsDisplayData()
	{
		List<GameLootRewardDisplayData> list = new List<GameLootRewardDisplayData>();
		if (!UIBase.gameState.recordManager.inGameMatterRewards.IsNullOrEmpty())
		{
			foreach (MatterInfo inGameMatterReward in UIBase.gameState.recordManager.inGameMatterRewards)
			{
				list.Add(GetMatterRewardDisplayData(inGameMatterReward));
			}
		}
		if (!UIBase.gameState.recordManager.gemConfigs.IsNullOrEmpty())
		{
			list.Add(GetGemRewardDisplayData());
		}
		GameLootRewardDisplayData classMatterReward = GetClassMatterReward();
		if (classMatterReward != null)
		{
			list.Add(classMatterReward);
		}
		return list;
	}

	private GameLootRewardDisplayData GetMatterRewardDisplayData(MatterInfo reward)
	{
		TextDataGenerator generator;
		return new GameLootRewardDisplayData
		{
			icon = reward.GlobalItemCellIcon,
			count = reward.count,
			description = reward.GetGlobalItemDescription(UIBase.applicationState, out generator)
		};
	}

	private GameLootRewardDisplayData GetGemRewardDisplayData()
	{
		return new GameLootRewardDisplayData
		{
			icon = gemIcon,
			count = UIBase.gameState.recordManager.gemConfigs.Sum((KeyValuePair<int, int> kv) => kv.Value),
			description = ScriptableObjectSingleton<TextDataPreset>.instance.term_gem_use_for_skill_upgrade_and_how_to_get
		};
	}

	private GameLootRewardDisplayData GetClassMatterReward()
	{
		MatterInfo matterInfo = UIBase.gameState.classMatterManager.GenerateGameOverMatters(UIBase.gameState.classMatterManager.currentEnteredNodePoint);
		if (matterInfo == null)
		{
			return null;
		}
		return GetMatterRewardDisplayData(matterInfo);
	}

	private void _OnEmptyAreaClicked()
	{
		SetDirty();
	}
}
