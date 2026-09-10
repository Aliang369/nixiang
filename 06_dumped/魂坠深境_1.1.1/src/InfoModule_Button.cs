using System;
using Common.UI;
using UnityEngine;

public class InfoModule_Button : InfoModule
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private RectTransform pivot;

	public override void Init()
	{
		if (!(button != null))
		{
			return;
		}
		button.onClick.AddListener(_OnButtonClicked);
		if (PlatformHelper.IsMobile())
		{
			return;
		}
		GameButton gameButton = button;
		gameButton.onHighlight = (Action)Delegate.Combine(gameButton.onHighlight, (Action)delegate
		{
			if (parent._setupData != null)
			{
				base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Hover, null, false));
			}
		});
		GameButton gameButton2 = button;
		gameButton2.onUnhighlight = (Action)Delegate.Combine(gameButton2.onUnhighlight, (Action)delegate
		{
			base.infoPanel?.HideCommonInfo(parent._setupData, CommonInfoData.ControlType.Hover);
		});
	}

	private void _OnButtonClicked()
	{
		if (parent._setupData != null)
		{
			base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Click, pivot, false));
		}
	}
}
