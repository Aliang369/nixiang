using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSetDisplay : CellUI<ItemSkinSetDef>, IDisplayModulesController
{
	public TextMeshProUGUI nameText;

	public Image icon;

	protected override void Init()
	{
		GetModule<RankModule>().fQuality = () => base.setupData.quality;
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase inputModuleBase)
		{
			if (inputModuleBase.pointerClicked)
			{
				ShowSkinSetPanel(base.setupData, CellUI<ItemSkinSetDef>.fortressState);
			}
		});
	}

	protected override void OnSetup(ItemSkinSetDef _)
	{
		nameText.text = base.setupData.skinSetName.ColorText(base.setupData.quality.GetQualityBaseColor());
		icon.sprite = base.setupData.icon;
	}

	public static void ShowSkinSetPanel(ItemSkinSetDef skinSetDef, FortressStateInfo fortressState)
	{
		if (fortressState.uiManager.itemSkinSetPanel != null)
		{
			fortressState.uiManager.DisposeUI(fortressState.uiManager.itemSkinSetPanel);
			UnityEngine.Object.Destroy(fortressState.uiManager.itemSkinSetPanel.gameObject);
			fortressState.uiManager.itemSkinSetPanel = null;
		}
		fortressState.uiManager.itemSkinSetPanel = UnityEngine.Object.Instantiate(skinSetDef.itemSkinSetPanel, fortressState.uiManager.itemSkinSetPanelRoot);
		fortressState.uiManager.InitUI(fortressState.uiManager.itemSkinSetPanel);
		fortressState.uiManager.itemSkinSetPanel.Setup(skinSetDef);
	}
}
