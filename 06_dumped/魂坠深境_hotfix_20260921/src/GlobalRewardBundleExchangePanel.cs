using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class GlobalRewardBundleExchangePanel : CellUI<MatterInfo>, ISelectablePanel
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI bundleReserveText;

	[SerializeField]
	private TextMeshProUGUI openBundleInfo;

	[SerializeField]
	private GlobalItemDisplay lootDisplay;

	[SerializeField]
	private GlobalItemDisplay prefab;

	[SerializeField]
	private CommonInfoPanel commonInfoPanel;

	[SerializeField]
	private GameObject configArea;

	[SerializeField]
	private RangeSliderWithAdjustButton slider;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private GlobalItemDisplay selected;

	private MatterInfo matterInfo => base.setupData;

	private PickLootSetup setup => matterInfo?.matterDef?.lootSetup as PickLootSetup;

	private int storageLootCount => CellUI<MatterInfo>.applicationState.matterManager.GetMatterCount(matterInfo.matterDef);

	private int maxCount => Mathf.Min(storageLootCount, 100);

	public int count => Mathf.Clamp(slider.CurrentValue, 1, maxCount);

	protected override void Init()
	{
		InitUI(commonInfoPanel);
		InitUI(lootDisplay);
		GetPrefabPool(ref pool, prefab);
		confirmButton.onClick.AddListener(OnConfirm);
		backButton.onClick.AddListener(base.Unset);
		slider.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
		void OnConfirm()
		{
			if (!(selected == null) && selected.setupData is GlobalRewardSetup item && setup != null && setup.globalRewards.Contains(item))
			{
				CellUI<MatterInfo>.applicationState.matterManager.OpenPickLoot(matterInfo.matterDef, count, setup.globalRewards.IndexOf(item));
				Unset();
			}
		}
	}

	protected override void OnSetup(MatterInfo setupData)
	{
		if (setup == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		pool.DespawnAll();
		foreach (GlobalRewardSetup globalReward in setup.globalRewards)
		{
			pool.Spawn(globalReward);
		}
		selected = null;
		lootDisplay.Setup(setupData);
		slider.MinValue = 1;
		slider.MaxValue = maxCount;
		bundleReserveText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_storage.FormatWith(storageLootCount);
		slider.CurrentValue = 1;
		ForceUpdate();
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		bool flag = selected != null;
		if (configArea.activeSelf != flag)
		{
			configArea.SetActive(flag);
		}
		if (!flag)
		{
			commonInfoPanel.HideAllCommonInfo();
			return;
		}
		openBundleInfo.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_comsume_loot_get_bonus.FormatWith(count, matterInfo.GlobalItemTitle, count, selected.setupData.GlobalItemTitle);
		commonInfoPanel.ShowCommonInfo(new CommonInfoData(selected.setupData, CommonInfoData.ControlType.Click, null, false));
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		if (TryGetRewardDisplay(module, out var rewardDisplay))
		{
			selected = rewardDisplay;
			SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		selected = null;
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return selected == module.parent as GlobalItemDisplay;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		GlobalItemDisplay rewardDisplay;
		return TryGetRewardDisplay(module, out rewardDisplay);
	}

	private bool TryGetRewardDisplay(SelectableModuleBase module, out GlobalItemDisplay rewardDisplay)
	{
		rewardDisplay = module.parent as GlobalItemDisplay;
		if (rewardDisplay?.setupData is GlobalRewardSetup item)
		{
			return setup?.globalRewards.Contains(item) ?? false;
		}
		return false;
	}
}
