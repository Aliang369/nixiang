using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using TMPro;
using UnityEngine;

public class DeckPanel : UIBase, ISelectablePanel
{
	private enum ShowType
	{
		None,
		Item,
		Influence,
		Skill,
		Clavis
	}

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private GameObject itemViewPort;

	[SerializeField]
	private ScrollRectGridLayoutGroup itemGridLayoutGroup;

	[SerializeField]
	private InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	private GameObject influenceViewPort;

	[SerializeField]
	private ScrollRectGridLayoutGroup influenceGridLayoutGroup;

	[SerializeField]
	private GameObject skillViewPort;

	[SerializeField]
	private GameSkillPresetArea skillPresetArea;

	[SerializeField]
	private SkillSlotPanel skillSlotPanel;

	[SerializeField]
	private ClavisPanel clavisPanel;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private SwitchButton toItemButton;

	[SerializeField]
	private SwitchButton toInfluenceButton;

	[SerializeField]
	private SwitchButton toSkillButton;

	[SerializeField]
	private SwitchButton toClavisButton;

	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private CharacterSpineDisplay characterDisplay;

	[SerializeField]
	private PauseButton pauseButton;

	[SerializeField]
	private ShowMapButton showMapButton;

	[SerializeField]
	public DeckInfoPanel deckInfoPanel;

	[SerializeField]
	public CharacterStrengthenArea characterStrengthenArea;

	[SerializeField]
	public GameObject characterStrengthenInfo;

	[SerializeField]
	private GameButton recommendPanelButton;

	[NonSerialized]
	private Item selectedItem;

	[NonSerialized]
	private ShowType showType;

	[NonSerialized]
	private ShowType targetShowType;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private GameSkill currentInspectedSkill;

	public GameSkill CurrentInspectedSkill
	{
		get
		{
			if (showType != ShowType.Skill || currentInspectedSkill == null)
			{
				return null;
			}
			GameStateInfo gameStateInfo = UIBase.gameState;
			if (gameStateInfo == null || gameStateInfo.playerState?.AllSkills.Contains(currentInspectedSkill) != true)
			{
				return null;
			}
			return currentInspectedSkill;
		}
	}

	protected override void Init()
	{
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinished;
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemDisplayPrefab.transform.parent);
		itemDisplayPool.ConfigInfoPanel(deckInfoPanel);
		influenceDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, influenceDisplayPrefab.transform.parent);
		influenceDisplayPool.ConfigInfoPanel(deckInfoPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		toItemButton.button.onClick.AddListener(_OnToItemButtonClicked);
		toInfluenceButton.button.onClick.AddListener(_OnToInfluenceButtonClicked);
		toSkillButton.button.onClick.AddListener(_OnToSkillButtonClicked);
		toClavisButton.button.onClick.AddListener(_OnClavisButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		GameSkillPresetArea gameSkillPresetArea = skillPresetArea;
		gameSkillPresetArea.onSelect = (Action<ProfileSkillPresetSlotCell>)Delegate.Combine(gameSkillPresetArea.onSelect, new Action<ProfileSkillPresetSlotCell>(_OnPresetSkillSelected));
		SkillSlotPanel obj = skillSlotPanel;
		obj.onSelectedChanged = (Action<SkillSlotDisplay>)Delegate.Combine(obj.onSelectedChanged, new Action<SkillSlotDisplay>(_OnActiveSkillSelected));
		recommendPanelButton.onClick.AddListener(delegate
		{
			UIBase.gameState.UIManager.recommendItemPanel.ShowItem(UIBase.gameState.playerState.character.def.attachClass);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "其他-卡组推荐界面", "背包");
		});
		InitUI(skillPresetArea);
		InitUI(characterDisplay);
		InitUI(skillSlotPanel);
		InitUI(clavisPanel);
		InitUI(pauseButton);
		InitUI(showMapButton);
		InitUI(deckInfoPanel);
		InitUI(characterStrengthenArea);
		characterStrengthenArea.gameObject.SetActive(value: true);
		characterStrengthenInfo.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinished;
		GameSkillPresetArea gameSkillPresetArea = skillPresetArea;
		gameSkillPresetArea.onSelect = (Action<ProfileSkillPresetSlotCell>)Delegate.Remove(gameSkillPresetArea.onSelect, new Action<ProfileSkillPresetSlotCell>(_OnPresetSkillSelected));
		SkillSlotPanel obj = skillSlotPanel;
		obj.onSelectedChanged = (Action<SkillSlotDisplay>)Delegate.Remove(obj.onSelectedChanged, new Action<SkillSlotDisplay>(_OnActiveSkillSelected));
		currentInspectedSkill = null;
	}

	public void Show()
	{
		UnselectAll();
		showType = ShowType.None;
		targetShowType = ShowType.Item;
		base.gameObject.SetActive(value: true);
	}

	protected override void UpdateDisplay()
	{
		characterStrengthenArea.Setup(UIBase.gameState.playerState.character);
		if ((UnityEngine.Object)(object)characterDisplay.skeletonGraphic == null)
		{
			characterDisplay.Setup(UIBase.gameState.playerState.character);
		}
		UpdatePanel();
	}

	private void UpdatePanel()
	{
		if (showType != targetShowType)
		{
			showType = targetShowType;
			itemViewPort.SetActive(showType == ShowType.Item);
			influenceViewPort.SetActive(showType == ShowType.Influence);
			skillViewPort.SetActive(showType == ShowType.Skill);
			clavisPanel.gameObject.SetActive(showType == ShowType.Clavis);
			toItemButton.Unselect();
			toInfluenceButton.Unselect();
			toSkillButton.Unselect();
			toClavisButton.Unselect();
			switch (showType)
			{
			case ShowType.Item:
				UpdateItemSequence();
				UpdateItemNumText();
				toItemButton.Select();
				break;
			case ShowType.Influence:
				UpdateInfluenceSequence();
				UpdateInfluenceNumText();
				toInfluenceButton.Select();
				break;
			case ShowType.Skill:
				UpdateSkillSequence();
				UpdateSkillNumText();
				toSkillButton.Select();
				break;
			case ShowType.Clavis:
				toClavisButton.Select();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			showMapButton.gameObject.SetActive(!UIBase.gameState.dungeonMapManager.inMapMode);
		}
	}

	public void TriggerItemSelect(Item item)
	{
		UnselectAll();
		selectedItem = item;
	}

	public void ShowItemReferenceDisplay(Item item)
	{
		SetDirty();
	}

	public void UnselectAll(bool isSelected = true)
	{
		if (isSelected)
		{
			selectedItem = null;
		}
		currentInspectedSkill = null;
		deckInfoPanel.HideAllCommonInfo();
		SetDirty();
	}

	public void RefreshCurrentSkillDisplay()
	{
		if (showType == ShowType.Skill)
		{
			skillPresetArea.Setup(UIBase.gameState.playerState.prepareSkillsPreset, UIBase.gameState.playerState.character);
			skillSlotPanel.SetDirty();
			deckInfoPanel.RefreshCurrentInfo();
			UIBase.gameState.UIManager.playerBftUIDisplay?.skillPanel?.SetDirty();
			SetDirty();
		}
	}

	private void UpdateItemSequence()
	{
		List<Item> list = new List<Item>(UIBase.gameState.playerState.DeckContainer.GetAllItems());
		list.Sort(delegate(Item item1, Item item2)
		{
			int num = string.Compare(item1.GetDef().itemName, item2.GetDef().itemName, StringComparison.Ordinal);
			return (num != 0) ? num : (item1.owner.GetRawIndex(item1) - item2.owner.GetRawIndex(item2));
		});
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, itemDisplayPool, itemGridLayoutGroup);
	}

	private void UpdateItemNumText()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(UIBase.gameState.playerState.DeckContainer.ItemsCount);
	}

	private void UpdateInfluenceSequence()
	{
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(UIBase.gameState.playerState.bft.influences, influenceDisplayPool, influenceGridLayoutGroup);
	}

	private void UpdateInfluenceNumText()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(UIBase.gameState.playerState.bft.influences.Count);
	}

	private void UpdateSkillSequence()
	{
		skillPresetArea.Setup(UIBase.gameState.playerState.prepareSkillsPreset, UIBase.gameState.playerState.character);
	}

	private void UpdateSkillNumText()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0_per_ag1.FormatWith(UIBase.gameState.playerState.ActivatedSkills.Count(), UIBase.gameState.playerState.skillLimit);
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		TriggerItemSelect((module.parent as ItemDisplayBase).item);
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		if (selectedItem == (module.parent as ItemDisplayBase).item)
		{
			UnselectAll();
		}
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return selectedItem == (module.parent as ItemDisplayBase).item;
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.battleItemInfoPanel.Hide();
		UnselectAll();
	}

	private void _OnToItemButtonClicked()
	{
		targetShowType = ShowType.Item;
		UnselectAll();
		SetDirty();
	}

	private void _OnToInfluenceButtonClicked()
	{
		targetShowType = ShowType.Influence;
		UnselectAll();
		SetDirty();
	}

	private void _OnToSkillButtonClicked()
	{
		targetShowType = ShowType.Skill;
		UnselectAll();
		SetDirty();
	}

	private void _OnClavisButtonClicked()
	{
		targetShowType = ShowType.Clavis;
		UnselectAll();
		SetDirty();
	}

	private void _OnEmptyClicked()
	{
		UnselectAll();
		characterStrengthenInfo.gameObject.SetActive(value: false);
	}

	private void _OnPresetSkillSelected(ProfileSkillPresetSlotCell cell)
	{
		currentInspectedSkill = (cell?.skillSlot as GameSkillSlot)?.gameSkill;
	}

	private void _OnActiveSkillSelected(SkillSlotDisplay display)
	{
		currentInspectedSkill = display?.skillSlot?.gameSkill;
	}

	private void _OnActionFrameFinished()
	{
		SetDirty();
	}
}
