using TMPro;
using UnityEngine.UI;

public static class ItemCostDisplayStyle
{
	public static void ApplyCostIcon(Image costImage, Item item)
	{
		if (!(costImage == null) && item != null)
		{
			costImage.color = item.GetCostImageColor();
			costImage.material = item.GetCostMaterial();
		}
	}

	public static void ApplyCostText(TextMeshProUGUI costText, Item item, int cost)
	{
		if (!(costText == null) && item != null)
		{
			costText.text = item.GetDisplayCostText(cost);
			costText.color = item.GetCostTextColor();
		}
	}
}
