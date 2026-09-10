using System.Collections.Generic;

public static class AssetCatalogExtension
{
	public static List<ItemDefinition> GetGlobalLootItems(this AssetCatalog catalog)
	{
		return catalog.itemDefCatalog.ValidAssets.FindAll((ItemDefinition it) => !it.developingAssetFlag && it.generationInfo != null && it.generationInfo.isLoot);
	}

	public static List<ItemDefinition> GetCharacterItems(this AssetCatalog catalog, CharacterClassType characterClass)
	{
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (ItemDefinition validAsset in catalog.itemDefCatalog.ValidAssets)
		{
			if (!validAsset.developingAssetFlag && validAsset.generationInfo != null && !validAsset.generationInfo.attachClass.IsNullOrEmpty() && validAsset.generationInfo.attachClass.Contains(characterClass))
			{
				list.Add(validAsset);
			}
		}
		return list;
	}
}
