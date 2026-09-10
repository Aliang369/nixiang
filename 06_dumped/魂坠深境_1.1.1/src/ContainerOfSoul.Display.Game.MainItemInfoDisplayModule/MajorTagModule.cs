using System;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class MajorTagModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI itemMajorTagText;

	[SerializeField]
	private GameObject itemTagsPivot;

	[SerializeField]
	private SkinSlotDisplay BigCard_BaseEffect_Back;

	[SerializeField]
	private SkinSlotDisplay BigCard_CardBase_Back;

	[SerializeField]
	private SkinSlotDisplay BigCard_IconEffect_Back;

	[SerializeField]
	private SkinSlotDisplay BigCard_IconEffect_Front;

	[SerializeField]
	private SkinSlotDisplay BigCard_CardBase_Front;

	[SerializeField]
	private SkinSlotDisplay BigCard_UpgradedImage;

	[SerializeField]
	private SkinSlotDisplay BigCard_NameImage;

	[SerializeField]
	private SkinSlotDisplay BigCard_SpellBackground;

	[SerializeField]
	private SkinSlotDisplay BigCard_CostIcon;

	[SerializeField]
	private SkinSlotDisplay BigCard_BaseEffect_Front;

	[SerializeField]
	private ItemTagCell itemTagCellPrefab;

	[SerializeField]
	private RectTransform itemTagCellParent;

	[NonSerialized]
	private CellUIPrefabPool<ItemTagCell, ItemTag> itemTagCellPool;

	[NonSerialized]
	private SkinSlotDisplay[] allSlot;

	public override void Init()
	{
		itemTagCellPool = base.mainItemInfoDisplay.GetPrefabPool<ItemTagCell, ItemTag>(itemTagCellPrefab, itemTagCellParent);
		allSlot = new SkinSlotDisplay[10] { BigCard_BaseEffect_Back, BigCard_CardBase_Back, BigCard_IconEffect_Back, BigCard_IconEffect_Front, BigCard_CardBase_Front, BigCard_UpgradedImage, BigCard_NameImage, BigCard_SpellBackground, BigCard_CostIcon, BigCard_BaseEffect_Front };
		SkinSlotDisplay[] array = allSlot;
		foreach (SkinSlotDisplay child in array)
		{
			parent.InitUI(child);
		}
	}

	public override void Setup()
	{
		if (base.item != null)
		{
			SetupMajorTag();
			SetupTags();
			SetupSkinSlots(base.item);
		}
	}

	private void SetupMajorTag()
	{
		itemMajorTagText.text = base.item.GetMajorTagText();
	}

	private void SetupTags()
	{
		itemTagCellPool.DespawnAll();
		bool active = false;
		foreach (ItemTag tag in base.item.GetTags())
		{
			if (!tag.majorTag && !tag.hideTag)
			{
				itemTagCellPool.Spawn(tag);
				active = true;
			}
		}
		itemTagsPivot.SetActive(active);
	}

	private void SetupSkinSlots(Item item)
	{
		BigCard_BaseEffect_Back.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_BaseEffect_Back);
		BigCard_CardBase_Back.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_CardBase_Back);
		BigCard_IconEffect_Back.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_IconEffect_Back);
		BigCard_IconEffect_Front.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_IconEffect_Front);
		BigCard_CardBase_Front.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_CardBase_Front);
		BigCard_UpgradedImage.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_UpgradedImage);
		BigCard_NameImage.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_NameImage);
		BigCard_SpellBackground.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_SpellBackground);
		BigCard_CostIcon.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_CostIcon);
		BigCard_BaseEffect_Front.OverrideSkin(item, ItemSkinBaseDef.ItemSkinBaseSlotType.BigCard_BaseEffect_Front);
	}

	private void SetSkin(ItemDefinition itemDefinition, ItemSkinBaseDef skinBaseDef)
	{
		SetupSkinSlots(new Item(itemDefinition));
	}
}
