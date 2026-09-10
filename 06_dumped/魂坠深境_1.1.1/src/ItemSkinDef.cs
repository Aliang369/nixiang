using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSkinDef : CTScriptableObject, IAssetIcon
{
	public enum ItemSkinGetSourceType
	{
		None,
		SeasonLevelLine,
		Challenge,
		Purchase
	}

	[CTSOAssetName]
	public string itemSkinID;

	public Sprite icon;

	public Sprite globalItemIcon;

	public Material iconMaterial;

	public QualityLevel quality;

	public ItemDefinition attachItem;

	public ItemSkinBaseDef defaultItemSkinBase;

	public ItemSkinSetDef attachSkinSet;

	public ItemSkinGetSourceType getSourceType;

	public bool gachaDrawable;

	public override string CTAssetIDPrepend => "ITSK";

	public string ItemSkinNameWithSet => attachItem?.itemName + "-" + attachSkinSet?.skinSetName;

	public int SetBatchIndex => SetBatchConfig?.batchIndex ?? 0;

	public ItemSkinSetDef.SkinSetBatchConfig SetBatchConfig => attachSkinSet?.batchConfigs.FirstOrDefault((ItemSkinSetDef.SkinSetBatchConfig c) => c.skinDefs.Contains(this));

	public Sprite IconAsset => icon;

	public static List<ItemSkinDef> GetAllSkinsOfItem(ItemDefinition itemDef, AssetCatalog catalog)
	{
		return catalog.itemSkinCatalog.ValidAssets.FindAll((ItemSkinDef skin) => skin.attachItem == itemDef);
	}

	public int GetSetSkinCost(ApplicationStateInfo applicationStateInfo)
	{
		return SetBatchConfig.GetCurrentSetSkinCoinCost(applicationStateInfo);
	}

	public Item GenerateSkinedItem()
	{
		Item item = new Item(attachItem).WithSkin(this);
		item.Upgrade();
		return item;
	}

	public bool IsUnlocked(ApplicationStateInfo applicationStateInfo)
	{
		return applicationStateInfo.itemSkinManager.IsItemSkinUnlocked(this);
	}
}
