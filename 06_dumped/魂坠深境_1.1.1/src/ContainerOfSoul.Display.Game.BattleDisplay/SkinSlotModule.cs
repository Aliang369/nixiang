using UnityEngine;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class SkinSlotModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private SkinSlotDisplay SmallCard_CardBase_Back;

	[SerializeField]
	private SkinSlotDisplay SmallCard_IconEffect_Back;

	[SerializeField]
	private SkinSlotDisplay SmallCard_IconEffect_Front;

	[SerializeField]
	private SkinSlotDisplay SmallCard_CardBase_Front;

	[SerializeField]
	private SkinSlotDisplay SmallCard_UpgradedImage;

	[SerializeField]
	private SkinSlotDisplay SmallCard_CostIcon;

	private ItemSkinDef currentSkin;

	private ItemSkinBaseDef currentSkinBase;

	public override void Init()
	{
		if (SmallCard_CardBase_Back != null)
		{
			parent.InitUI(SmallCard_CardBase_Back);
		}
		if (SmallCard_IconEffect_Back != null)
		{
			parent.InitUI(SmallCard_IconEffect_Back);
		}
		if (SmallCard_IconEffect_Front != null)
		{
			parent.InitUI(SmallCard_IconEffect_Front);
		}
		if (SmallCard_CardBase_Front != null)
		{
			parent.InitUI(SmallCard_CardBase_Front);
		}
		if (SmallCard_UpgradedImage != null)
		{
			parent.InitUI(SmallCard_UpgradedImage);
		}
		if (SmallCard_CostIcon != null)
		{
			parent.InitUI(SmallCard_CostIcon);
		}
	}

	public override void Setup()
	{
		SetupSkin();
	}

	public override void UpdateDisplay()
	{
		if (currentSkin != base.item.skin || currentSkinBase != base.item.skinBase)
		{
			currentSkin = base.item.skin;
			currentSkinBase = base.item.skinBase;
			SetupSkin();
		}
	}

	private void SetupSkin()
	{
		SmallCard_CardBase_Back?.Setup(base.item);
		SmallCard_IconEffect_Back?.Setup(base.item);
		SmallCard_IconEffect_Front?.Setup(base.item);
		SmallCard_CardBase_Front?.Setup(base.item);
		SmallCard_UpgradedImage?.Setup(base.item);
		SmallCard_CostIcon?.Setup(base.item);
		SmallCard_CardBase_Back?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_CardBase_Back);
		SmallCard_IconEffect_Back?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_IconEffect_Back);
		SmallCard_IconEffect_Front?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_IconEffect_Front);
		SmallCard_CardBase_Front?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_CardBase_Front);
		SmallCard_UpgradedImage?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_UpgradedImage);
		SmallCard_CostIcon?.OverrideSkin(base.item, ItemSkinBaseDef.ItemSkinBaseSlotType.SmallCard_CostIcon);
	}
}
