using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using TMPro;
using UnityEngine;

public class RecommendItemPanel : UIBase, ISelectablePanel
{
	[SerializeField]
	private RecommendItemGroup group;

	[SerializeField]
	private ItemDisplayBase item;

	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	private SwitchTag itemTag;

	[SerializeField]
	private SwitchTag recommendTag;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameObject itemPanel;

	[SerializeField]
	private GameObject recommendPanel;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	public ItemInfoPanel itemInfoPanel;

	[SerializeField]
	public TextMeshProUGUI unlockItemsCountText;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<RecommendItemGroup, RecommendItemGroup.Config> pool;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemPool;

	[NonSerialized]
	private SelectableModuleBase selected;

	private CharacterClassDef selectedClass
	{
		get
		{
			return characterClassFilterPanel.selected.setupData.target as CharacterClassDef;
		}
		set
		{
			characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(value);
		}
	}

	protected override void Init()
	{
		InitUI(switchPanel);
		InitUI(itemInfoPanel);
		GetPrefabPool(ref pool, group);
		GetPrefabPool(ref itemPool, item, null, itemInfoPanel);
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
		characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(UIBase.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets[0]);
		characterClassFilterPanel.onChangeSelected = delegate
		{
			selected = null;
			SetDirty();
		};
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirtyWithFadeAnim(active: false);
		});
		switchPanel.Select(recommendTag);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, (Action)delegate
		{
			selected = null;
			itemInfoPanel.HideAllCommonInfo();
			SetDirty();
		});
		UIHelper.RegisterEmptyAreaClickedEvent(OnEmptyClicked);
	}

	protected override void Dispose()
	{
		UIHelper.UnregisterEmptyAreaClickedEvent(OnEmptyClicked);
	}

	public void ShowItem(CharacterClassDef characterClass)
	{
		selected = null;
		SetActiveAndDirtyWithFadeAnim(active: true);
		switchPanel.Select(recommendTag);
		selectedClass = characterClass;
		SetDirty();
	}

	public void ShowRecommend(CharacterClassDef characterClass = null)
	{
		selected = null;
		SetActiveAndDirtyWithFadeAnim(active: true);
		switchPanel.Select(recommendTag);
		if (characterClass != null)
		{
			selectedClass = characterClass;
		}
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "其他-卡组推荐界面");
	}

	protected override void UpdateDisplay()
	{
		itemPanel.gameObject.SetActive(value: false);
		recommendPanel.gameObject.SetActive(value: false);
		if (switchPanel.targetShowTag == itemTag)
		{
			List<ItemDefinition> poolItems = selectedClass.GetPoolItems(UIBase.applicationState);
			itemPool.UpdateSequenceDisplay(poolItems, layoutGroup, ItemDisplayBase.itemDefinitionConvertor);
			itemPanel.gameObject.SetActive(value: true);
			int num = poolItems.Count((ItemDefinition it) => UIBase.applicationState.discoverManager.IsDiscovered(it));
			unlockItemsCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_index_unlock_count_arg0_per_arg1.FormatWith(num, poolItems.Count);
		}
		else if (switchPanel.targetShowTag == recommendTag)
		{
			pool.Sync(selectedClass.recommendItemConfig);
			recommendPanel.gameObject.SetActive(value: true);
		}
	}

	private void OnEmptyClicked()
	{
		selected = null;
		SetDirty();
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		selected = module;
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		selected = null;
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return (selected?.parent as ItemDisplayBase)?.item?.def == (module.parent as ItemDisplayBase)?.item.def;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		ItemDisplayBase itemDisplayBase = module.parent as ItemDisplayBase;
		if (itemDisplayBase.GetModule<IndexModule>() == null)
		{
			return true;
		}
		return itemDisplayBase.item.def.IsDiscovered(UIBase.applicationState);
	}
}
