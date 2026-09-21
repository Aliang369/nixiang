using System;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Character;

public class ItemDisplayPanel : UIBase
{
	[SerializeField]
	private ItemDisplayBase itemDisplayPrefabPrefab;

	[SerializeField]
	private Transform itemCellParent;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	protected override void Init()
	{
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefabPrefab, itemCellParent);
	}

	public void Setup(CharacterClassDef characterInfo)
	{
		itemDisplayPool.DespawnAll();
		foreach (ItemDefinition item in UIBase.fortressState.applicationState.assetCatalog.itemDefCatalog.ValidAssets.FindAll((ItemDefinition item) => item?.generationInfo?.attachClass != null && item.generationInfo.attachClass.Contains(characterInfo.mClass)))
		{
			itemDisplayPool.Spawn(new Item(item));
		}
	}
}
