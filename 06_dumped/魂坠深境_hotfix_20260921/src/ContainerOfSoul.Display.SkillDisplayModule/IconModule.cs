using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.SkillDisplayModule;

public class IconModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	public Image backgroundImage;

	[SerializeField]
	public Sprite hardBackGroundSprite;

	[SerializeField]
	public Sprite virtualBackGroundSprite;

	public bool alwaysUseHardBackground;

	public override void Setup()
	{
		skillIcon.sprite = base.gameSkill.Icon;
		if (UIDisplayModule.gameState != null)
		{
			if (alwaysUseHardBackground || base.gameSkill.IsInSlotSkill(UIDisplayModule.gameState))
			{
				backgroundImage.sprite = hardBackGroundSprite;
			}
			else
			{
				backgroundImage.sprite = virtualBackGroundSprite;
			}
		}
	}
}
