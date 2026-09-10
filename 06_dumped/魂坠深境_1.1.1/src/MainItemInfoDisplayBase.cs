using System;
using Common.UI;
using UnityEngine;

public class MainItemInfoDisplayBase : CellUI<Item>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected MainItemInfoDisplayBase mainItemInfoDisplay => parent as MainItemInfoDisplayBase;

		protected Item item => mainItemInfoDisplay.item;
	}

	[SerializeField]
	public RectTransform backgroundVFXPivot;

	[NonSerialized]
	public bool isUpgradeDisplay;

	[NonSerialized]
	public Item item;

	[NonSerialized]
	public ItemInfoDisplayBase itemInfoDisplay;

	[NonSerialized]
	public bool setAsUpgradeDisplay;

	[NonSerialized]
	public ViewUpgradeButtonPreviewSetup viewUpgradeButtonPreviewSetup;

	protected override void Init()
	{
		itemInfoDisplay = parent as ItemInfoDisplayBase;
		base.transform.SetSiblingIndex(2);
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase inputModuleBase)
		{
			if (CellUI<Item>.gameState != null)
			{
				if (inputModuleBase.pointerEnter)
				{
					CellUI<Item>.gameState.UIManager.pointerOverItemInfoDisplay = itemInfoDisplay;
				}
				if (inputModuleBase.pointerExit)
				{
					CellUI<Item>.gameState.UIManager.pointerOverItemInfoDisplay = null;
				}
			}
			if (CellUI<Item>.fortressState != null)
			{
				if (inputModuleBase.pointerEnter)
				{
					CellUI<Item>.fortressState.uiManager.pointerOverItemInfoDisplay = itemInfoDisplay;
				}
				if (inputModuleBase.pointerExit)
				{
					CellUI<Item>.fortressState.uiManager.pointerOverItemInfoDisplay = null;
				}
			}
		});
	}

	protected override void OnSetup(Item dailyContract)
	{
		viewUpgradeButtonPreviewSetup = null;
		item = dailyContract;
		if (dailyContract != null)
		{
			isUpgradeDisplay = setAsUpgradeDisplay;
			setAsUpgradeDisplay = false;
		}
	}

	protected override void OnUnset()
	{
		viewUpgradeButtonPreviewSetup = null;
	}

	public string GetDisplayItemName()
	{
		return UncollectedItemDisplayHelper.GetDisplayItemName(CellUI<Item>.applicationState, item, itemInfoDisplay?.showUncollectedStatus ?? false);
	}
}
