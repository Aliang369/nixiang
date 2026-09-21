using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class CostModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private Image costImage;

	[SerializeField]
	private TextMeshProUGUI costText;

	public override void UpdateDisplay()
	{
		if (!base.item.HasSpell)
		{
			costImage.gameObject.SetActive(value: false);
			return;
		}
		int displayFinalCost = base.item.GetDisplayFinalCost(UIDisplayModule.applicationState, base.mainItemInfoDisplay.itemInfoDisplay.triggerType);
		costImage.sprite = base.item.GetCostImageLarge();
		ItemCostDisplayStyle.ApplyCostIcon(costImage, base.item);
		costImage.gameObject.SetActive(value: true);
		ItemCostDisplayStyle.ApplyCostText(costText, base.item, displayFinalCost);
	}
}
