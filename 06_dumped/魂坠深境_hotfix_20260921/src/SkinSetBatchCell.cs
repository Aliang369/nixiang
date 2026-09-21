using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSetBatchCell : CellUI<ItemSkinSetBatch>, IDisplayModulesController
{
	public TextMeshProUGUI nameText;

	public Image icon;

	protected override void Init()
	{
		GetModule<RankModule>().fQuality = () => base.setupData.skinSetDef.quality;
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase inputModuleBase)
		{
			if (inputModuleBase.pointerClicked)
			{
				ShowSkinSetPanel(base.setupData.skinSetDef, CellUI<ItemSkinSetBatch>.fortressState);
			}
		});
	}

	protected override void OnSetup(ItemSkinSetBatch _)
	{
		nameText.text = base.setupData.BatchConfig.GetDisplayName(base.setupData.skinSetDef, withDotSplit: true);
		icon.sprite = base.setupData.skinSetDef.icon;
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
