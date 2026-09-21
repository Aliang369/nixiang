using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class UpgradedModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image upgradedImage;

	public override void UpdateDisplay()
	{
		upgradedImage.gameObject.SetActive(base.slotDisplay.item.upgraded);
	}
}
