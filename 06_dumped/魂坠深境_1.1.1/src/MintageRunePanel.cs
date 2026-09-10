using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;
using TMPro;
using UnityEngine;

public abstract class MintageRunePanel : UIBase, ISelectablePanel
{
	public enum MintageStateReason
	{
		None,
		WithoutEnoughRuneSlot,
		ConstrainNotAccept,
		WithRuneNotStackable,
		ForceMintageWithCrystal,
		MintageAble
	}

	[SerializeField]
	private GameButton mintageRuneButton;

	[SerializeField]
	private TextMeshProUGUI mintageRuneDescriptionText;

	[SerializeField]
	protected GameButton switchRuneButton;

	[SerializeField]
	protected TextMeshProUGUI switchRuneCostText;

	[SerializeField]
	private RectTransform runeCommonReferencePivot;

	[SerializeField]
	private CommonReference runeCommonReferencePrefab;

	[SerializeField]
	protected ItemDisplayArea itemArea;

	[SerializeField]
	protected GameObject hintPanel;

	[SerializeField]
	protected TextMeshProUGUI hintPanelText;

	[SerializeField]
	private GameButton hintConfirmButton;

	[SerializeField]
	private GameButton hintCancelButton;

	[SerializeField]
	private RuneDisplay runeDisplayPrefab;

	[SerializeField]
	private RectTransform itemInfoDisplayArea;

	[SerializeField]
	private ItemInfoDisplay_Normal itemInfoDisplay;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> runeDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<CommonReference, CommonInfoData> runeCommonReferencePool;

	[NonSerialized]
	private List<RuneDisplay> runeDisplays;

	[NonSerialized]
	protected Item selected;

	private bool GlobalWithoutRuneAttachCountLimit => UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.WithoutRuneAttachCountLimit);

	private int GlobalRuneCountLimitDelta => UIBase.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.RuneAttachCountLimitDelta);

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selected == item;
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selected == item)
		{
			Unselect();
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selected != item)
		{
			selected = item;
			SetDirty();
		}
	}

	bool ISelectablePanel.IsSelectDark(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		MintageStateReason reason;
		return !CanMintage(item, out reason);
	}

	protected override void Init()
	{
		InitUI(itemArea);
		hintPanel.gameObject.SetActive(value: false);
		mintageRuneButton.onClick.AddListener(_OnMintageRuneButtonClicked);
		hintConfirmButton.onClick.AddListener(_OnHintConfirmButtonClicked);
		hintCancelButton.onClick.AddListener(_OnHintCancelButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		runeDisplayPrefab.gameObject.SetActive(value: false);
		runeDisplayPool = GetPrefabPool<RuneDisplay, Rune>(runeDisplayPrefab, runeDisplayPrefab.transform.parent);
		runeDisplays = new List<RuneDisplay>();
		runeCommonReferencePool = GetPrefabPool<CommonReference, CommonInfoData>(runeCommonReferencePrefab, runeCommonReferencePrefab.transform.parent);
		itemInfoDisplayArea.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		List<Rune> runes = GetRunes();
		for (int i = 0; i < runes.Count; i++)
		{
			if (runeDisplays.Count < runes.Count)
			{
				runeDisplays.Add(runeDisplayPool.Spawn((runes.Count > i) ? runes[i] : null));
			}
			else if (runes.Count > i)
			{
				runeDisplays[i].Setup(runes[i]);
			}
			else
			{
				runeDisplays[i].Unset();
			}
		}
		while (runeDisplays.Count > runes.Count)
		{
			RuneDisplay runeDisplay = runeDisplays[runeDisplays.Count - 1];
			runeDisplayPool.Despawn(runeDisplay);
			runeDisplays.Remove(runeDisplay);
		}
		runeCommonReferencePool.DespawnAll();
		runeCommonReferencePool.Spawn(runes.Select((Rune r) => new CommonInfoData(r, CommonInfoData.ControlType.Click, null, false)));
		if (runes.Count > 0 && selected != null)
		{
			itemInfoDisplayArea.gameObject.SetActive(value: true);
			if (itemInfoDisplay.item != selected)
			{
				itemInfoDisplay.Setup(selected);
				itemInfoDisplay.mainItemInfoDisplay.GetComponent<RuneModule_MintagePreview>().SetPreviewRunes(runes);
			}
			bool interactable = CanMintage(selected, out var reason);
			mintageRuneButton.gameObject.SetActive(value: true);
			mintageRuneButton.interactable = interactable;
			mintageRuneDescriptionText.gameObject.SetActive(value: true);
			TextMeshProUGUI textMeshProUGUI = mintageRuneDescriptionText;
			textMeshProUGUI.text = reason switch
			{
				MintageStateReason.None => "", 
				MintageStateReason.WithoutEnoughRuneSlot => ScriptableObjectSingleton<TextDataPreset>.instance.term_reject_attach_rune_with_already, 
				MintageStateReason.ConstrainNotAccept => ScriptableObjectSingleton<TextDataPreset>.instance.term_reject_attach_rune_not_accept, 
				MintageStateReason.WithRuneNotStackable => ScriptableObjectSingleton<TextDataPreset>.instance.term_reject_attach_rune_with_unstackable_rune, 
				MintageStateReason.ForceMintageWithCrystal => ScriptableObjectSingleton<TextDataPreset>.instance.term_force_attach_rune_with_arg0_crystal.FormatWith(GetForceMintageRunePrice(selected).ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.crystalColor)), 
				MintageStateReason.MintageAble => ScriptableObjectSingleton<TextDataPreset>.instance.term_attach_arg0_rune_to_this_card.FormatWith(GetRunesString(runes)), 
				_ => "", 
			};
		}
		else if (selected != null)
		{
			itemInfoDisplayArea.gameObject.SetActive(value: true);
			itemInfoDisplay.Setup(selected);
			mintageRuneButton.gameObject.SetActive(value: false);
			mintageRuneDescriptionText.gameObject.SetActive(value: false);
		}
		else
		{
			itemInfoDisplayArea.gameObject.SetActive(value: false);
			itemInfoDisplay.Unset();
		}
	}

	public void Unselect()
	{
		if (selected != null)
		{
			selected = null;
			SetDirty();
		}
	}

	protected bool WithEnoughRuneSlot(Item item)
	{
		if (GlobalWithoutRuneAttachCountLimit)
		{
			return true;
		}
		return item.Runes.Count < item.runeSlot + GlobalRuneCountLimitDelta;
	}

	private string GetRunesString(List<Rune> runes)
	{
		string text = "";
		for (int i = 0; i < runes.Count; i++)
		{
			if (i != 0)
			{
				text += " ";
			}
			text += runes[i].GetNameWithValue();
		}
		return text;
	}

	protected int GetForceMintageRunePrice(Item item)
	{
		return ScriptableObjectSingleton<GameDataManager>.instance.mintageRuneRoomConfig.forceMintageRunePricePerRune * item.Runes.Count;
	}

	protected virtual void MintageRune(Item item, List<Rune> runes)
	{
		UIBase.gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			runes = runes.ConvertAll((Rune r) => (r: r, item: item, value: r.value))
		}));
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			ClosePanel();
		}));
	}

	public abstract List<Rune> GetRunes();

	protected virtual bool CanMintage(Item item, out MintageStateReason reason)
	{
		List<Rune> runes = GetRunes();
		reason = MintageStateReason.None;
		if (runes.Count == 0)
		{
			return false;
		}
		if (runes.Any((Rune rune2) => rune2.def.CheckCanAddRuneTo(item) != true))
		{
			reason = MintageStateReason.ConstrainNotAccept;
			return false;
		}
		foreach (Rune rune in runes)
		{
			if (!rune.def.stacking && item.Runes.Any((Rune r) => r.def == rune.def))
			{
				reason = MintageStateReason.WithRuneNotStackable;
				return false;
			}
		}
		return true;
	}

	protected virtual void ClosePanel()
	{
	}

	protected virtual void _OnMintageRuneButtonClicked()
	{
		UIHelper.DisableAllButton();
		Item selectedItem = selected;
		itemInfoDisplay.mainItemInfoDisplay.GetComponent<RuneModule_MintagePreview>().OnMintage();
		UIBase.gameState.actionManager.EnqueueAction(new WaitAction(0.8f));
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			Unselect();
			MintageRune(selectedItem, GetRunes());
			SetDirty();
			UIHelper.EnableAllButton();
		}));
	}

	protected virtual void _OnHintConfirmButtonClicked()
	{
		Item item = selected;
		Unselect();
		hintPanel.gameObject.SetActive(value: false);
		MintageRune(item, GetRunes());
		SetDirty();
	}

	private void _OnHintCancelButtonClicked()
	{
		hintPanel.gameObject.SetActive(value: false);
		selected = null;
		SetDirty();
	}

	private void _OnEmptyClicked()
	{
		hintPanel.gameObject.SetActive(value: false);
		Unselect();
	}
}
