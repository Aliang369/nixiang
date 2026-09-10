using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class CharacterHeadBadgeModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private GameObject badgeRoot;

	[SerializeField]
	private Image frameImage;

	[SerializeField]
	private Image headImage;

	public override void Setup()
	{
		Hide();
		UpdateDisplay();
	}

	public override void UpdateDisplay()
	{
		if (base.pathLocationDisplay.location == UIDisplayModule.gameState?.dungeonMapManager?.currentLocation)
		{
			ShowCurrentCharacterHead();
		}
		else
		{
			Hide();
		}
	}

	private void ShowCurrentCharacterHead()
	{
		Sprite sprite = UIDisplayModule.gameState?.playerState?.character?.def?.globalItemIcon;
		if (sprite == null || badgeRoot == null || headImage == null)
		{
			Hide();
			return;
		}
		headImage.sprite = sprite;
		headImage.preserveAspect = true;
		if (frameImage != null)
		{
			frameImage.enabled = true;
		}
		badgeRoot.SetActive(value: true);
	}

	private void Hide()
	{
		if (headImage != null)
		{
			headImage.sprite = null;
		}
		if (badgeRoot != null)
		{
			badgeRoot.SetActive(value: false);
		}
	}
}
