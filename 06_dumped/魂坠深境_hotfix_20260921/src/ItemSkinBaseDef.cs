using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSkinBaseDef : CTScriptableObject
{
	public enum ItemSkinBaseSlotType
	{
		BigCard_BaseEffect_Back = 1001,
		BigCard_CardBase_Back = 1002,
		BigCard_IconEffect_Back = 1003,
		BigCard_IconEffect_Front = 1004,
		BigCard_CardBase_Front = 1005,
		BigCard_UpgradedImage = 1006,
		BigCard_NameImage = 1007,
		BigCard_SpellBackground = 1008,
		BigCard_CostIcon = 1009,
		BigCard_BaseEffect_Front = 10010,
		SmallCard_CardBase_Back = 2001,
		SmallCard_IconEffect_Back = 2002,
		SmallCard_IconEffect_Front = 2003,
		SmallCard_CardBase_Front = 2004,
		SmallCard_UpgradedImage = 2005,
		SmallCard_CostIcon = 2006
	}

	public struct ConfigRow
	{
		public ItemSkinBaseSlotType type;

		public ItemSkinBaseSlotSetup defaultSetup;

		public ItemSkinBaseSlotSetup attack;

		public ItemSkinBaseSlotSetup ability;

		public ItemSkinBaseSlotSetup defense;

		public ItemSkinBaseSlotSetup companion;

		public ItemSkinBaseSlotSetup curse;
	}

	public struct ItemSkinBaseSlotSetup
	{
		public ImageSetup imageSetup;

		public SubItemSetup subitemSetup;
	}

	public class ImageSetup
	{
		[SerializeField]
		public Sprite m_Sprite;

		[SerializeField]
		public Color m_Color = Color.white;

		[SerializeField]
		public Material m_Material;

		[SerializeField]
		public Image.Type m_Type;

		[SerializeField]
		public Image.FillMethod m_FillMethod = Image.FillMethod.Radial360;

		[SerializeField]
		public int m_FillOrigin;

		[SerializeField]
		public bool m_FillClockwise = true;

		[SerializeField]
		public float m_FillAmount = 1f;

		[SerializeField]
		public bool m_FillCenter = true;

		[SerializeField]
		public bool m_PreserveAspect;

		[SerializeField]
		public bool m_UseSpriteMesh;

		[SerializeField]
		public float m_PixelsPerUnitMultiplier = 1f;

		[SerializeField]
		public bool m_RaycastTarget;

		[SerializeField]
		public Vector4 m_RaycastPadding = Vector4.zero;

		[SerializeField]
		public bool m_Maskable = true;

		public void Apply(Image image)
		{
			image.sprite = m_Sprite;
			image.color = m_Color;
			image.material = m_Material;
			image.type = m_Type;
			image.fillMethod = m_FillMethod;
			image.fillOrigin = m_FillOrigin;
			image.fillClockwise = m_FillClockwise;
			image.fillAmount = m_FillAmount;
			image.fillCenter = m_FillCenter;
			image.preserveAspect = m_PreserveAspect;
			image.useSpriteMesh = m_UseSpriteMesh;
			image.pixelsPerUnitMultiplier = m_PixelsPerUnitMultiplier;
			image.raycastTarget = m_RaycastTarget;
			image.raycastPadding = m_RaycastPadding;
			image.maskable = m_Maskable;
		}
	}

	public class SubItemSetup
	{
		public GameObject subObject;
	}

	[CTSOAssetName]
	public string ItemSkinBaseName;

	public List<ConfigRow> slotSetups = new List<ConfigRow>();

	public override string CTAssetIDPrepend => "ITSKBASE";

	public ImageSetup GetImageSetup(ItemSkinBaseSlotType type, ItemTag itemTag)
	{
		ConfigRow configRow = slotSetups.Find((ConfigRow s) => s.type == type);
		ImageSetup imageSetup;
		switch (itemTag?.type ?? ((ItemTag.Type)0))
		{
		case ItemTag.Type.CompanionDefence:
		case ItemTag.Type.Companion:
			imageSetup = configRow.companion.imageSetup;
			break;
		case ItemTag.Type.Attack:
			imageSetup = configRow.attack.imageSetup;
			break;
		case ItemTag.Type.Defence:
			imageSetup = configRow.defense.imageSetup;
			break;
		case ItemTag.Type.Curse:
			imageSetup = configRow.curse.imageSetup;
			break;
		case ItemTag.Type.Ability:
			imageSetup = configRow.ability.imageSetup;
			break;
		default:
			imageSetup = configRow.defaultSetup.imageSetup;
			break;
		}
		if (imageSetup == null)
		{
			return configRow.defaultSetup.imageSetup;
		}
		return imageSetup;
	}

	public SubItemSetup GetSubItemSetupSetup(ItemSkinBaseSlotType type, ItemTag itemTag)
	{
		ConfigRow configRow = slotSetups.Find((ConfigRow s) => s.type == type);
		SubItemSetup subitemSetup;
		switch (itemTag.type)
		{
		case ItemTag.Type.CompanionDefence:
		case ItemTag.Type.Companion:
			subitemSetup = configRow.companion.subitemSetup;
			break;
		case ItemTag.Type.Attack:
			subitemSetup = configRow.attack.subitemSetup;
			break;
		case ItemTag.Type.Defence:
			subitemSetup = configRow.defense.subitemSetup;
			break;
		case ItemTag.Type.Curse:
			subitemSetup = configRow.curse.subitemSetup;
			break;
		case ItemTag.Type.Ability:
			subitemSetup = configRow.ability.subitemSetup;
			break;
		default:
			subitemSetup = configRow.defaultSetup.subitemSetup;
			break;
		}
		if (subitemSetup == null)
		{
			return configRow.defaultSetup.subitemSetup;
		}
		return subitemSetup;
	}

	private void OnInspectorInit()
	{
		if (slotSetups == null)
		{
			slotSetups = new List<ConfigRow>();
		}
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_BaseEffect_Back);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_CardBase_Back);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_IconEffect_Back);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_IconEffect_Front);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_CardBase_Front);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_UpgradedImage);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_NameImage);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_SpellBackground);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_CostIcon);
		AddUniqueRow(ItemSkinBaseSlotType.BigCard_BaseEffect_Front);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_CardBase_Back);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_IconEffect_Back);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_IconEffect_Front);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_CardBase_Front);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_UpgradedImage);
		AddUniqueRow(ItemSkinBaseSlotType.SmallCard_CostIcon);
		void AddUniqueRow(ItemSkinBaseSlotType type)
		{
			if (!slotSetups.Contains((ConfigRow r) => r.type == type))
			{
				slotSetups.Add(new ConfigRow
				{
					type = type
				});
			}
		}
	}
}
