using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class ItemSkinManager : GlobalGameManager
{
	public class ItemSkinState
	{
		[SaveField(null, null)]
		public ItemSkinDef skinDef;

		[SaveField(null, null)]
		public int level;
	}

	[SaveField(null, null)]
	public List<ItemSkinState> unlockedItemSkinLevels;

	[SaveField(null, null)]
	public List<ItemSkinBaseDef> unlockedItemSkinBases;

	[SaveField(null, null)]
	public List<ItemSkinDef> unviewedChangedItemSkins;

	private Dictionary<ItemSkinDef, int> unlockedItemSkinLevelsCache;

	private int ItemSkinMaxLevel => ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinMaxLevel;

	public bool HasUnviewedChangedItemSkin
	{
		get
		{
			if (unviewedChangedItemSkins != null)
			{
				return unviewedChangedItemSkins.Count > 0;
			}
			return false;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (unlockedItemSkinLevels == null)
		{
			unlockedItemSkinLevels = new List<ItemSkinState>();
		}
		if (unlockedItemSkinBases == null)
		{
			unlockedItemSkinBases = new List<ItemSkinBaseDef>();
		}
		if (unviewedChangedItemSkins == null)
		{
			unviewedChangedItemSkins = new List<ItemSkinDef>();
		}
		RebuildCache();
	}

	public void RebuildCache()
	{
		unlockedItemSkinLevelsCache = unlockedItemSkinLevels.ToDictionary((ItemSkinState s) => s.skinDef, (ItemSkinState s) => s.level);
	}

	public void AddItemSkin(ItemSkinDef itemSkin, bool addToRecommendation = true)
	{
		ItemSkinState itemSkinState = unlockedItemSkinLevels.Find((ItemSkinState s) => s.skinDef == itemSkin);
		if (itemSkinState != null)
		{
			if (itemSkinState.level < ItemSkinMaxLevel)
			{
				itemSkinState.level++;
				unlockedItemSkinLevelsCache[itemSkin] = itemSkinState.level;
				unviewedChangedItemSkins.AddUnique(itemSkin);
			}
		}
		else
		{
			ItemSkinState item = new ItemSkinState
			{
				skinDef = itemSkin,
				level = 0
			};
			unlockedItemSkinLevels.Add(item);
			unlockedItemSkinLevelsCache[itemSkin] = 0;
			unviewedChangedItemSkins.AddUnique(itemSkin);
			if (addToRecommendation && itemSkin.attachItem != null)
			{
				base.applicationState.guideManager.itemSkinRecommendedItems.AddUnique(itemSkin.attachItem);
			}
			if (itemSkin.IsValidAsset())
			{
				base.applicationState.eventManager.unlockItemSkinEvent.TriggerEvent(itemSkin);
			}
		}
		base.applicationState.globalUnlockManager.RecordGotObject(itemSkin);
		base.applicationState.savingManager.SetProfileDirtyToSave();
	}

	public void UnlockItemSkinBase(ItemSkinBaseDef itemSkinBase)
	{
		if (!IsItemSkinBaseUnlocked(itemSkinBase))
		{
			unlockedItemSkinBases.Add(itemSkinBase);
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public bool IsItemSkinUnlocked(ItemSkinDef itemSkin)
	{
		return unlockedItemSkinLevelsCache.ContainsKey(itemSkin);
	}

	public int GetItemSkinLevel(ItemSkinDef itemSkin)
	{
		if (!unlockedItemSkinLevelsCache.TryGetValue(itemSkin, out var value))
		{
			return -1;
		}
		return value;
	}

	public bool IsItemSkinBaseUnlocked(ItemSkinBaseDef itemSkinBase)
	{
		return unlockedItemSkinBases.Contains(itemSkinBase);
	}

	public bool HasUnviewedChangedItemSkinInSet(ItemSkinSetDef skinSetDef)
	{
		if (skinSetDef == null || unviewedChangedItemSkins == null || unviewedChangedItemSkins.Count == 0)
		{
			return false;
		}
		return unviewedChangedItemSkins.Any((ItemSkinDef skin) => skin != null && skin.attachSkinSet == skinSetDef);
	}

	public void ClearUnviewedChangedItemSkinsInSet(ItemSkinSetDef skinSetDef)
	{
		if (!(skinSetDef == null) && unviewedChangedItemSkins != null && unviewedChangedItemSkins.Count != 0 && unviewedChangedItemSkins.RemoveAll((ItemSkinDef skin) => skin == null || skin.attachSkinSet == skinSetDef) > 0)
		{
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public ItemSkinDef DrawRandomSkin(QualityLevel filterQuality = QualityLevel.None)
	{
		List<ItemSkinDef> list = base.applicationState.assetCatalog.itemSkinCatalog.ValidAssets.Where((ItemSkinDef c) => c.gachaDrawable).ToList();
		if (filterQuality != QualityLevel.None)
		{
			list = list.FindAll((ItemSkinDef s) => s.quality == filterQuality);
		}
		return list.SelectRandom();
	}

	public List<ItemSkinDef> GetAllValidSkinsOfItem(ItemDefinition item)
	{
		return ItemSkinDef.GetAllSkinsOfItem(item, base.applicationState.assetCatalog).FindAll((ItemSkinDef s) => s.IsValidAsset());
	}

	public List<ItemSkinDef> GetAllIndexVisibleSkinsOfItem(ItemDefinition item)
	{
		return GetAllValidSkinsOfItem(item).FindAll(delegate(ItemSkinDef skin)
		{
			ItemSkinSetDef.SkinSetBatchConfig setBatchConfig = skin.SetBatchConfig;
			return setBatchConfig == null || !setBatchConfig.hideInIndex;
		});
	}

	public List<ItemSkinDef> GetAllIndexVisibleUnlockedSkinsOfItem(ItemDefinition item)
	{
		return GetAllIndexVisibleSkinsOfItem(item).FindAll(IsItemSkinUnlocked);
	}

	public List<ItemSkinDef> GetAllUnlockedSkinsOfItem(ItemDefinition item)
	{
		return GetAllValidSkinsOfItem(item).FindAll(IsItemSkinUnlocked);
	}

	public ItemSkinDef GetItemActiveSkin(ItemDefinition item)
	{
		List<ItemSkinDef> allUnlockedSkinsOfItem = GetAllUnlockedSkinsOfItem(item);
		if (allUnlockedSkinsOfItem == null || allUnlockedSkinsOfItem.Count == 0)
		{
			return null;
		}
		return allUnlockedSkinsOfItem.OrderByDescending(GetItemSkinLevel).First();
	}

	public ItemSkinBaseDef GetItemActiveSkinBase(ItemDefinition item)
	{
		ItemSkinDef itemActiveSkin = GetItemActiveSkin(item);
		object obj = itemActiveSkin?.attachSkinSet?.skinBase;
		if (obj == null)
		{
			if ((object)itemActiveSkin == null)
			{
				return null;
			}
			obj = itemActiveSkin.defaultItemSkinBase;
		}
		return (ItemSkinBaseDef)obj;
	}
}
