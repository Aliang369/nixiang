using System;
using System.Linq;
using Common.UI;
using UnityEngine;

public class ItemSkinSetSelectPanel : CellUI<MatterInfo>, ISelectablePanel
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private CellUIPrefabPoolConfig<SkinSelectCell, RandomItemSkinInSkinSetReward> pool;

	[NonSerialized]
	private SkinSelectCell selected;

	private MatterInfo matterInfo => base.setupData;

	private PickLootSetup setup => matterInfo?.matterDef?.lootSetup as PickLootSetup;

	protected override void Init()
	{
		Init(pool);
		confirmButton.onClick.AddListener(OnConfirm);
		backButton.onClick.AddListener(((CellUI<MatterInfo>)this).HideUI<MatterInfo>);
		void OnConfirm()
		{
			if (!(selected == null))
			{
				GlobalRewardSetup globalRewardSetup = selected.setupData;
				if (globalRewardSetup != null && setup != null && setup.globalRewards.Contains(globalRewardSetup))
				{
					CellUI<MatterInfo>.applicationState.matterManager.OpenPickLoot(matterInfo.matterDef, 1, setup.globalRewards.IndexOf(globalRewardSetup));
					this.HideUI();
				}
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
		pool.Sync(setup.globalRewards.Select((GlobalRewardSetup r) => r as RandomItemSkinInSkinSetReward).ToList());
		selected = null;
		SetDirty();
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		bool interactable = selected != null;
		confirmButton.interactable = interactable;
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		if (TryGetRewardDisplay(module, out var rewardDisplay))
		{
			CellUI<MatterInfo>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
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
		return selected == module.parent as SkinSelectCell;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		return (module.parent as SkinSelectCell).batch.lockedCount > 0;
	}

	private bool TryGetRewardDisplay(SelectableModuleBase module, out SkinSelectCell rewardDisplay)
	{
		rewardDisplay = module.parent as SkinSelectCell;
		RandomItemSkinInSkinSetReward randomItemSkinInSkinSetReward = rewardDisplay?.setupData;
		if (randomItemSkinInSkinSetReward != null)
		{
			return setup?.globalRewards.Contains(randomItemSkinInSkinSetReward) ?? false;
		}
		return false;
	}
}
