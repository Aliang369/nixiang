using UnityEngine;

public class ToStoreButton : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private int firstIndex;

	[SerializeField]
	private int secondIndex;

	protected override void Init()
	{
		button.onClick.AddListener(delegate
		{
			if (UIBase.fortressState != null && UIBase.fortressState.uiManager.purchaseUIPanel.TryShowSwitchPanelIndex(firstIndex, secondIndex))
			{
				UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
			}
		});
	}

	protected override void UpdateDisplay()
	{
		if (button != null)
		{
			GameButton gameButton = button;
			FortressStateInfo fortressStateInfo = UIBase.fortressState;
			gameButton.interactable = fortressStateInfo != null && fortressStateInfo.uiManager?.purchaseUIPanel?.CanOpenPurchasePanel == true;
		}
	}
}
