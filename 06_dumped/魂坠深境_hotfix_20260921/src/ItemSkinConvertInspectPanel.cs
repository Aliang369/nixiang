using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class ItemSkinConvertInspectPanel : UIBase
{
	public ItemSkinConvertInspectCell inspectCellPrefab;

	public RectTransform inspectCellParent;

	public GameButton backButton;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<ItemSkinConvertInspectCell, ItemSkinDef> cellPool;

	protected override void Init()
	{
		cellPool = GetPrefabPool<ItemSkinConvertInspectCell, ItemSkinDef>(inspectCellPrefab, inspectCellParent);
		CellUIPrefabPool<ItemSkinConvertInspectCell, ItemSkinDef> cellUIPrefabPool = cellPool;
		cellUIPrefabPool.onInit = (Action<ItemSkinConvertInspectCell>)Delegate.Combine(cellUIPrefabPool.onInit, (Action<ItemSkinConvertInspectCell>)delegate(ItemSkinConvertInspectCell cell)
		{
			cell.SetInfoPanel(UIBase.gameState.UIManager.universalCommonInfoPanel);
		});
		backButton.onClick.AddListener(Hide);
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		SetActiveAndDirty(active: true);
	}

	protected override void UpdateDisplay()
	{
		HashSet<ItemDefinition> characterPoolItems = UIBase.gameState.playerState.CharacterDef.attachClass.GetPoolItems(UIBase.applicationState).ToHashSet();
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay((from s in UIBase.applicationState.itemSkinManager.unlockedItemSkinLevels
			select s.skinDef into s
			where characterPoolItems.Contains(s.attachItem)
			select s).ToList(), cellPool, layoutGroup);
		layoutGroup.RefreshLayout();
	}

	public void Hide()
	{
		UIBase.gameState.UIManager.universalCommonInfoPanel.HideAll();
		base.gameObject.SetActive(value: false);
	}
}
