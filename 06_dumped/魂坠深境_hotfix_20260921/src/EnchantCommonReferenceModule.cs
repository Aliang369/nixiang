using Common.UI;
using UnityEngine;

public class EnchantCommonReferenceModule : EnchantDisplay.DisplayModule
{
	[SerializeField]
	protected GameButton imageButton;

	[SerializeField]
	protected RectTransform pivotRectTransform;

	public override void Init()
	{
		imageButton?.onClick.AddListener(_OnImageButtonClicked);
	}

	private void _OnImageButtonClicked()
	{
		UIDisplayModule.gameState.UIManager.universalCommonInfoPanel.AppendInfo(new CommonInfoData(base.enchant, CommonInfoData.ControlType.Click, pivotRectTransform, false));
	}
}
