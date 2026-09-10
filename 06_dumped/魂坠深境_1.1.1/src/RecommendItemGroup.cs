using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

public class RecommendItemGroup : CellUI<RecommendItemGroup.Config>
{
	public class Config
	{
		[SerializeField]
		[LocalizeText]
		public string title;

		[SerializeField]
		[LocalizeText]
		public string description;

		[SerializeField]
		public List<ItemDefinition> items;
	}

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private ItemDisplayBase itemDisplay;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> pool;

	protected override void Init()
	{
		ICommonInfoPanel commonInfoPanel = (parent as RecommendItemPanel)?.itemInfoPanel;
		ICommonInfoPanel targetInfoPanel = commonInfoPanel ?? UIHelper.GetDefaultCommonInfoPanel();
		UncollectedStatusCommonInfoPanel infoPanel = new UncollectedStatusCommonInfoPanel(() => targetInfoPanel);
		GetPrefabPool(ref pool, itemDisplay, null, infoPanel);
	}

	protected override void OnSetup(Config setupData)
	{
		title.text = setupData.title;
		description.text = setupData.description;
		pool.DespawnAll();
		List<ItemDefinition> list = ((setupData.items == null) ? null : new List<ItemDefinition>(setupData.items));
		list?.Sort((ItemDefinition a, ItemDefinition b) => ItemDefinition.ItemDefIndexComparer(a, b));
		pool.Sync(list, ItemDisplayBase.itemDefinitionConvertor);
		SetDirty();
	}
}
