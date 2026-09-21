using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdolLevelRewardCell : CellUI<IdolLevelRewardCell.Data>
{
	public class Data
	{
		public IdolState idolState;

		public GlobalRewardSetup reward;
	}

	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private Image classRewardBackground;

	[SerializeField]
	private Image globalRewardBackground;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI rewardDescription;

	protected override void Init()
	{
		InitUI(globalItemDisplay);
	}

	protected override void OnSetup(Data setupData)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		GlobalRewardSetup reward = base.setupData.reward;
		globalItemDisplay.Setup(reward);
		classRewardBackground.gameObject.SetActive(value: false);
		globalRewardBackground.gameObject.SetActive(value: false);
		if (IsClassReward(reward))
		{
			classRewardBackground.gameObject.SetActive(value: true);
		}
		else
		{
			globalRewardBackground.gameObject.SetActive(value: true);
		}
		(string, string) rewardDetail = GetRewardDetail(reward);
		if (!rewardDetail.Item1.IsNullOrEmpty())
		{
			title.text = rewardDetail.Item1;
		}
		else
		{
			title.text = GetTitle(reward);
		}
		SetText(rewardDescription, rewardDetail.Item2);
	}

	private string GetTitle(GlobalRewardSetup reward)
	{
		if (reward is UnlockCTSOGlobalReward { unlocks: var unlocks } && !unlocks.IsEmptyOrNull())
		{
			if (unlocks.All((CTScriptableObject u) => u is ItemDefinition))
			{
				return CellUI<Data>.textData.term_idol_unlock_card_title;
			}
			if (unlocks.All((CTScriptableObject u) => u is InfluenceDef))
			{
				return CellUI<Data>.textData.term_idol_unlock_blessing_title;
			}
			if (unlocks.All((CTScriptableObject u) => u is RuneDef))
			{
				return CellUI<Data>.textData.term_idol_unlock_rune_title;
			}
		}
		if (reward is ForgetShrinePointReward forgetShrinePointReward)
		{
			return ForgetShrinePointReward.GetName(forgetShrinePointReward.characterClassDef);
		}
		return "";
	}

	private (string name, string description) GetRewardDetail(GlobalRewardSetup reward)
	{
		if (!(reward is UnlockCTSOGlobalReward unlockCTSOGlobalReward) || unlockCTSOGlobalReward.unlocks.IsEmptyOrNull())
		{
			return (name: "", description: "");
		}
		if (unlockCTSOGlobalReward.unlocks.All((CTScriptableObject u) => u is ItemDefinition))
		{
			List<ItemDefinition> source = unlockCTSOGlobalReward.unlocks.Cast<ItemDefinition>().ToList();
			return (name: string.Join("\n", source.Select(GetItemName)), description: string.Join("\n", from s in source.Select(GetItemDescription)
				where !string.IsNullOrEmpty(s)
				select s));
		}
		if (unlockCTSOGlobalReward.unlocks.All((CTScriptableObject u) => u is InfluenceDef))
		{
			List<InfluenceDef> source2 = unlockCTSOGlobalReward.unlocks.Cast<InfluenceDef>().ToList();
			return (name: string.Join("\n", source2.Select((InfluenceDef influence) => influence.InfluenceName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(influence.quality)))), description: string.Join("\n", from influence in source2
				select influence.GetInfluenceDescription(CellUI<Data>.applicationState, out var _) into s
				where !string.IsNullOrEmpty(s)
				select s));
		}
		if (unlockCTSOGlobalReward.unlocks.All((CTScriptableObject u) => u is RuneDef))
		{
			List<RuneDef> source3 = unlockCTSOGlobalReward.unlocks.Cast<RuneDef>().ToList();
			return (name: string.Join("\n", source3.Select((RuneDef rune) => rune.Name.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(rune.quality)))), description: string.Join("\n", from rune in source3
				select rune.GetDescription(CellUI<Data>.applicationState) into s
				where !string.IsNullOrEmpty(s)
				select s));
		}
		return (name: "", description: "");
	}

	private string GetItemName(ItemDefinition itemDefinition)
	{
		Item item = new Item(itemDefinition);
		return item.GetItemName().ColorText(item.GetItemNameColor(qualityColor: true));
	}

	private string GetItemDescription(ItemDefinition itemDefinition)
	{
		if (itemDefinition == null)
		{
			return "";
		}
		Item item = new Item(itemDefinition);
		List<string> list = new List<string>();
		string text = TextDataPreset.FormatItemSpellDescription(CellUI<Data>.applicationState, item, out var generator);
		if (!string.IsNullOrEmpty(text))
		{
			list.Add(text);
		}
		string text2 = TextDataPreset.FormatItemPassiveDescription(CellUI<Data>.applicationState, item, out generator);
		if (!string.IsNullOrEmpty(text2))
		{
			list.Add(text2);
		}
		return string.Join("\n", list);
	}

	private bool IsClassReward(GlobalRewardSetup reward)
	{
		return IdolLevelRewardUtility.IsClassReward(reward);
	}

	private static void SetText(TextMeshProUGUI text, string value)
	{
		if (!(text == null))
		{
			text.gameObject.SetActive(!string.IsNullOrEmpty(value));
			text.text = value ?? "";
		}
	}
}
