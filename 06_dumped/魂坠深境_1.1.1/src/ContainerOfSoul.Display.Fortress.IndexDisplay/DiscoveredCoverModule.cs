using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class DiscoveredCoverModule : ItemDisplayBase.DisplayModule
{
	public Image undiscoveredCoverImage;

	public override void UpdateDisplay()
	{
		if (base.slotDisplay.item != null && base.slotDisplay.indexHide)
		{
			undiscoveredCoverImage.gameObject.SetActive(value: true);
		}
		else
		{
			undiscoveredCoverImage.gameObject.SetActive(value: false);
		}
	}
}
