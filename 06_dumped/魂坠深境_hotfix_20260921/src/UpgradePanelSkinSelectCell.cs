using System;
using TMPro;
using UnityEngine;

public class UpgradePanelSkinSelectCell : CellUI<UpgradePanelSkinSelectCell.CellData>
{
	public class CellData
	{
		public ItemSkinDef skin;

		public CellData(ItemSkinDef skin)
		{
			this.skin = skin;
		}
	}

	public TextMeshProUGUI skinNameText;

	public GameButton button;

	public GameObject darkenMask;

	[NonSerialized]
	private new UpgradePanel parent;

	protected override void Init()
	{
		parent = GetUIInParents<UpgradePanel>();
		button.onClick.AddListener(delegate
		{
			parent.upgradeTargetSkin = base.setupData.skin;
			parent.SetDirty();
		});
		button.onNotInteractableClick.AddListener(delegate
		{
			CellUI<CellData>.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_you_not_yest_unlock_skin_arg1_of_item_arg0.FormatWith(base.setupData.skin.attachItem.itemName, base.setupData.skin.attachSkinSet.SkinSetNameWithColor));
			parent.upgradeTargetSkin = base.setupData.skin;
			parent.SetDirty();
		});
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData?.skin != null)
		{
			skinNameText.text = base.setupData.skin.attachSkinSet.skinSetName;
			skinNameText.color = base.setupData.skin.attachSkinSet.skinSetMainColor;
		}
		else
		{
			skinNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_base_upgrade;
			skinNameText.color = Color.white;
		}
		if (base.setupData != null)
		{
			darkenMask.SetActive(parent.upgradeTargetSkin != base.setupData.skin);
		}
		GameButton gameButton = button;
		CellData cellData = base.setupData;
		gameButton.interactable = cellData != null && cellData.skin?.IsUnlocked(CellUI<CellData>.applicationState) == true;
	}
}
