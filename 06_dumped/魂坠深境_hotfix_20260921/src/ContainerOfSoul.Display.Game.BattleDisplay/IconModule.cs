using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class IconModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image itemImage;

	public RectTransform ItemImageRectTransform
	{
		get
		{
			if (!(itemImage != null))
			{
				return null;
			}
			return itemImage.rectTransform;
		}
	}

	public override void Setup()
	{
		UpdateDisplay();
	}

	public override void UpdateDisplay()
	{
		itemImage.sprite = base.slotDisplay.item.GetSkinIcon(UIDisplayModule.applicationState);
		itemImage.color = (base.slotDisplay.indexHide ? ColorHelper.IndexUnDiscoverColor : Color.white);
	}
}
