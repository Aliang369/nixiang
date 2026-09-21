using Common.UI;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSpineDisplay : UIBase, IDisplayModulesController
{
	public RectTransform skeletonGraphicParent;

	public Image skinImage;

	public SkeletonGraphic skeletonGraphic;

	public PlayerCharacterState character;

	protected override void Init()
	{
		skinImage.gameObject.SetActive(value: false);
	}

	public void Setup(PlayerCharacterState state)
	{
		if (state == character)
		{
			if ((Object)(object)skeletonGraphic != null)
			{
				((Graphic)(object)skeletonGraphic).color = skinImage.color;
			}
			return;
		}
		character = state;
		if ((Object)(object)skeletonGraphic != null)
		{
			Object.Destroy(((Component)(object)skeletonGraphic).gameObject);
		}
		skinImage.gameObject.SetActive(value: false);
		if ((Object)(object)state.def.spine != null)
		{
			skeletonGraphic = Object.Instantiate<SkeletonGraphic>(state.def.spine, (Transform)skeletonGraphicParent);
		}
		else
		{
			skinImage.gameObject.SetActive(value: true);
			skinImage.sprite = state.def.fullBody;
		}
		if ((Object)(object)skeletonGraphic != null)
		{
			((Graphic)(object)skeletonGraphic).color = skinImage.color;
		}
	}
}
