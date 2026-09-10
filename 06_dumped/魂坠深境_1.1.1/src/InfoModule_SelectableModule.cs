using System;
using Common.UI;
using UnityEngine;

[RequireComponent(typeof(SelectableModuleBase))]
public class InfoModule_SelectableModule : InfoModule
{
	[SerializeField]
	private RectTransform passRect;

	[NonSerialized]
	private SelectableModuleBase selectableModule;

	public override void Init()
	{
		selectableModule = GetComponent<SelectableModuleBase>();
		SelectableModuleBase selectableModuleBase = selectableModule;
		selectableModuleBase.onSelect = (Action)Delegate.Combine(selectableModuleBase.onSelect, (Action)delegate
		{
			base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Click, passRect, false));
		});
		SelectableModuleBase selectableModuleBase2 = selectableModule;
		selectableModuleBase2.onUnselect = (Action)Delegate.Combine(selectableModuleBase2.onUnselect, (Action)delegate
		{
			base.infoPanel?.HideCommonInfo(parent._setupData, CommonInfoData.ControlType.Click);
		});
	}
}
