using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class UpgradedModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	public Image upgradedImage;

	public override void UpdateDisplay()
	{
		upgradedImage.gameObject.SetActive(base.item.upgraded);
	}
}
