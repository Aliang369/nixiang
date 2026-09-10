using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

public class ForerunnerSelectPanel : UIBase, ISelectablePanel
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private SwitchButton toItemButton;

	[SerializeField]
	private SwitchButton toSkillButton;

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private GameObject itemViewPort;

	[SerializeField]
	private ScrollRectGridLayoutGroup itemGridLayoutGroup;

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private GameObject skillViewPort;

	[SerializeField]
	private ScrollRectGridLayoutGroup skillGridLayoutGroup;

	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	private TextMeshProUGUI numText;

	[NonSerialized]
	private ForerunnerPanel parentPanel;

	[NonSerialized]
	private ForerunnerLegacy forerunnerLegacy;

	[NonSerialized]
	private Item selectedItem;

	[NonSerialized]
	public GameSkill selectedSkill;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> skillDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private GameButton activeButton;

	protected override void Init()
	{
		InitUI(itemInfoDisplay);
		InitUI(skillInfoDisplay);
		skillDisplayPool = GetPrefabPool<SkillDisplay, GameSkill>(skillDisplay, skillDisplay.transform.parent);
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemDisplayPrefab.transform.parent);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		toItemButton.button.onClick.AddListener(_OnToItemButtonClicked);
		toSkillButton.button.onClick.AddListener(_OnToSkillButtonClicked);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		parentPanel = GetUIInParents<ForerunnerPanel>();
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		if (activeButton == toItemButton.button)
		{
			UpdateItemSequence();
		}
		if (activeButton == toSkillButton.button)
		{
			UpdateSkillSequence();
		}
		confirmButton.gameObject.SetActive(selectedItem != null || selectedSkill != null);
	}

	public void Setup(ForerunnerLegacy t)
	{
		forerunnerLegacy = t;
		_OnToItemButtonClicked();
		SetDirty();
	}

	public void Unset()
	{
	}

	private void UpdateItemSequence()
	{
		itemViewPort.SetActive(value: true);
		skillViewPort.SetActive(value: false);
		List<Item> list = new List<Item>(UIBase.gameState.playerState.DeckContainer.GetAllItems());
		list.Sort(delegate(Item item1, Item item2)
		{
			int num = string.Compare(item1.GetDef().itemName, item2.GetDef().itemName, StringComparison.Ordinal);
			return (num != 0) ? num : (item1.owner.GetRawIndex(item1) - item2.owner.GetRawIndex(item2));
		});
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, itemDisplayPool, itemGridLayoutGroup);
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(itemDisplayPool.activeCount);
	}

	private void UpdateSkillSequence()
	{
		itemViewPort.SetActive(value: false);
		skillViewPort.SetActive(value: true);
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(new List<GameSkill>(UIBase.gameState.playerState.ActivatedSkills), skillDisplayPool, skillGridLayoutGroup);
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(skillDisplayPool.activeCount);
	}

	public void UnselectAll()
	{
		selectedItem = null;
		selectedSkill = null;
		itemInfoDisplay.Unset();
		skillInfoDisplay.Unset();
		SetDirty();
	}

	private void _OnToItemButtonClicked()
	{
		activeButton = toItemButton.button;
		UnselectAll();
		toItemButton.Unselect();
		toSkillButton.Unselect();
		toItemButton.Select();
		SetDirty();
	}

	private void _OnToSkillButtonClicked()
	{
		activeButton = toSkillButton.button;
		UnselectAll();
		toItemButton.Unselect();
		toSkillButton.Unselect();
		toSkillButton.Select();
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}

	private void _OnConfirmButtonClicked()
	{
		forerunnerLegacy.SetItem(selectedItem);
		forerunnerLegacy.SetSkill(selectedSkill);
		parentPanel.forerunnerLeaveNotePanel.Setup(forerunnerLegacy);
		parentPanel.forerunnerLeaveNotePanel.SetActiveAndDirty(active: true);
	}

	private void _OnEmptyAreaClicked()
	{
		UnselectAll();
	}

	public void Select(SelectableModuleBase module)
	{
		UnselectAll();
		ItemDisplayBase component = module.GetComponent<ItemDisplayBase>();
		SkillDisplay component2 = module.GetComponent<SkillDisplay>();
		if (component != null)
		{
			selectedItem = component.item;
			itemInfoDisplay.Setup(selectedItem);
		}
		if (component2 != null)
		{
			selectedSkill = component2.setupData;
			skillInfoDisplay.Setup(selectedSkill);
		}
		SetDirty();
	}

	public void Unselect(SelectableModuleBase module)
	{
		UnselectAll();
	}

	public bool IsSelected(SelectableModuleBase module)
	{
		ItemDisplayBase component = module.GetComponent<ItemDisplayBase>();
		SkillDisplay component2 = module.GetComponent<SkillDisplay>();
		if (component != null)
		{
			return selectedItem == component.item;
		}
		if (component2 != null)
		{
			return selectedSkill == component2.setupData;
		}
		return false;
	}
}
