using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class ItemModule_Game : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI itemName;

	[SerializeField]
	private Image itemImage;

	public override void Setup()
	{
		itemImage.sprite = base.item.GetSkinIcon(UIDisplayModule.applicationState);
		Color itemNameColor = base.item.GetItemNameColor(qualityColor: true);
		itemName.color = itemNameColor;
		itemName.text = base.mainItemInfoDisplay.GetDisplayItemName();
		ItemQualityNameEcho.Setup(itemName, base.item.qualityLevel, itemNameColor, base.item.skin == null);
	}
}
