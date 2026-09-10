using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class ItemIndexPanel : IndexPagePanel, ISelectablePanel
{
	private class FilterData
	{
		public IconObjectFilterPanel.IconFilterData displayData;

		public IndexBookPage page;
	}

	[SerializeField]
	private ItemDisplayBase itemCellPrefab;

	[SerializeField]
	private IconObjectFilterPanel filterPanel;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private TextMeshProUGUI filterNameText;

	[NonSerialized]
	private List<FilterData> filterDatas;

	[NonSerialized]
	private FortressIndexUIPanel indexPanel;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemCellPool;

	[NonSerialized]
	private Item selectingItem;

	private IndexBookPage currentPage = IndexBookPage.ClassItem_Warrior;

	private IEnumerable<(CharacterClassDef, IndexBookPage)> ToDisplayCharacterClassPages => from p in UIBase.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets
		select (p: p, DiscoverManager.GetIndexPageByCharacterClass(p.mClass)) into p
		where ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.Any((IndexBookConfig c) => c.page == p.Item2)
		select p;

	public override string TASubPanelTagName => "档案馆-图鉴-卡牌";

	public override IndexBookPage GetIndexPageType()
	{
		return currentPage;
	}

	public override bool WithIndexBookRewardToGet()
	{
		if (!ToDisplayCharacterClassPages.Any(((CharacterClassDef, IndexBookPage) p) => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(p.Item2)))
		{
			return UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(IndexBookPage.Item_EnemyLoot);
		}
		return true;
	}

	public override void ClearTransientSelection()
	{
		selectingItem = null;
		indexPanel?.itemInfoPanel.HideAllCommonInfo();
		SetDirty();
	}

	public override bool TrySelectFirstRewardPage()
	{
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData data) => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(data.page));
		if (filterData == null)
		{
			return false;
		}
		IconObjectFilter filter = filterPanel.GetFilter(filterData.displayData.target);
		if (filter == null)
		{
			return false;
		}
		if (filterPanel.selected != filter)
		{
			filterPanel.selected = filter;
			filterPanel.OnChangeSelected();
		}
		return true;
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data as Item)?.GetDef());
	}

	public override bool EmphasizeNextRewardTarget()
	{
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData data) => data.page != currentPage && UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(data.page));
		IconObjectFilter iconObjectFilter = ((filterData == null) ? null : filterPanel.GetFilter(filterData.displayData.target));
		if (iconObjectFilter == null)
		{
			return false;
		}
		iconObjectFilter.transform.DOKill();
		iconObjectFilter.transform.DOPunchScale(Vector3.one * 0.1f, 0.45f, 2, 0.35f);
		return true;
	}

	public void Select(Item item)
	{
		selectingItem = item;
		SetDirty();
	}

	public void Unselect(Item item)
	{
		selectingItem = null;
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectingItem == item;
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		Unselect(item);
		(parent as FortressIndexUIPanel).itemInfoPanel.HideAllCommonInfo();
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		Select(item);
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return UIBase.applicationState.discoverManager.IsDiscovered(item.GetDef());
	}

	protected override void Init()
	{
		indexPanel = (FortressIndexUIPanel)parent;
		itemCellPool = GetPrefabPool<ItemDisplayBase, Item>(itemCellPrefab, itemCellPrefab.transform.parent);
		itemCellPool.ConfigInfoPanel((parent as FortressIndexUIPanel).itemInfoPanel);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		InitUI(filterPanel);
		InitFilterData();
		filterPanel.Sync(filterDatas.ConvertAll((FilterData f) => f.displayData));
		filterPanel.canUnselect = false;
		filterPanel.selected = filterPanel.GetFilter(filterDatas[0].displayData.target);
		filterPanel.onChangeSelected = _OnFilterChanged;
		_OnFilterChanged();
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	private void InitFilterData()
	{
		filterDatas = (from data in UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_items)
			select ToFilterData(data, DiscoverManager.GetIndexPageByCharacterClass((data.target as CharacterClassDef).mClass))).ToList();
		filterDatas.Add(ToFilterData(new IconObjectFilterPanel.IconFilterData(IndexBookPage.Item_EnemyLoot, ScriptableObjectSingleton<GameDataManager>.instance.generalClassIcon, ScriptableObjectSingleton<TextDataPreset>.instance.term_index_other_items, () => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(IndexBookPage.Item_EnemyLoot)), IndexBookPage.Item_EnemyLoot));
	}

	private void _OnFilterChanged()
	{
		ClearTransientSelection();
		object target = filterPanel.selected?.setupData.target;
		FilterData filterData = filterDatas?.FirstOrDefault((FilterData d) => object.Equals(d.displayData.target, target));
		if (filterData == null && filterDatas != null && filterDatas.Count > 0)
		{
			filterData = filterDatas[0];
		}
		if (filterData != null)
		{
			currentPage = filterData.page;
			indexPanel.SetDirty();
			List<Item> list = (from def in GetIndexPageAllContent<ItemDefinition>()
				select new Item(def)).ToList();
			list.Sort((Item a, Item b) => ItemDefinition.ItemDefIndexComparer(a.def, b.def, UIBase.applicationState));
			ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, itemCellPool, layoutGroup);
		}
	}

	protected override void UpdateDisplay()
	{
		filterNameText.text = ((base.CurrentPageConfig != null) ? base.CurrentPageConfig.pageTitle : "");
		layoutGroup.RefreshLayout();
	}

	private void _OnEmptyAreaClicked()
	{
		if (selectingItem != null)
		{
			Unselect(selectingItem);
		}
	}

	private FilterData ToFilterData(IconObjectFilterPanel.IconFilterData data, IndexBookPage page)
	{
		FilterData filterData = new FilterData();
		filterData.displayData = data;
		filterData.page = page;
		filterData.displayData.shouldShowRedDot = () => UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(page);
		return filterData;
	}
}
