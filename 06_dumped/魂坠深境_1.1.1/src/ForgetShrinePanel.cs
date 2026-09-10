using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgetShrinePanel : CellUI<CharacterClassState>
{
	[SerializeField]
	private Transform leftPoolContainer;

	[SerializeField]
	private Transform rightPoolContainer;

	[SerializeField]
	private GameButton applyButton;

	[SerializeField]
	private GameObject confirmPanel;

	[SerializeField]
	private TextMeshProUGUI confirmText;

	[SerializeField]
	private TextMeshProUGUI pointText;

	[SerializeField]
	private TextMeshProUGUI cardDeckTitle;

	[SerializeField]
	private TextMeshProUGUI forgetShrinePointTitle;

	[SerializeField]
	private TextMeshProUGUI sealCardsTitle;

	[SerializeField]
	private TextMeshProUGUI sealedCardsHint;

	[SerializeField]
	private Image forgetShrineIcon;

	[SerializeField]
	private CellUIPrefabPoolConfig<ForgetShrineItemSlot, ForgetShrineItemSlot.Data> deckItemSlotPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<ForgetShrineItemSlot, ForgetShrineItemSlot.Data> shrineItemSlotPool;

	[SerializeField]
	public CellUIPrefabPoolConfig<ItemDisplayBase, Item> itemDisplayPool;

	[SerializeField]
	public RectTransform deckRect;

	[SerializeField]
	public ScrollRectGridLayoutGroup deckLayoutGroup;

	[SerializeField]
	public ScrollRectGridLayoutGroup shrineLayoutGroup;

	[SerializeField]
	public RectTransform forgetShineRect;

	[SerializeField]
	public RectTransform dragPivot;

	[SerializeField]
	public Image deckRectImage;

	[SerializeField]
	public Image forgetShineRectImage;

	[NonSerialized]
	public List<ForgetShrineItemSlot.Data> deckSlots;

	[NonSerialized]
	public List<ForgetShrineItemSlot.Data> shrineSlots;

	[NonSerialized]
	public List<Item> deckItems;

	[NonSerialized]
	public List<Item> shrineItems;

	[NonSerialized]
	public bool hasChanges;

	[NonSerialized]
	public bool sequenceChanged;

	[NonSerialized]
	public Canvas canvas;

	private CharacterClassState characterClassState => base.setupData;

	public int forgetShrinePoint => characterClassState.forgetShrinePoint;

	private CharacterClassType currentClassType => characterClassState.classDef.mClass;

	protected override void Init()
	{
		Init(deckItemSlotPool);
		Init(shrineItemSlotPool);
		Init(itemDisplayPool);
		deckSlots = new List<ForgetShrineItemSlot.Data>();
		shrineSlots = new List<ForgetShrineItemSlot.Data>();
		deckItems = new List<Item>();
		shrineItems = new List<Item>();
		hasChanges = false;
		applyButton.onClick.AddListener(OnApplyButtonClick);
		canvas = GetComponentInParent<Canvas>(includeInactive: true);
		sealCardsTitle.text = CellUI<CharacterClassState>.textData.term_forget_shrine_seal_cards_title;
		sealedCardsHint.text = CellUI<CharacterClassState>.textData.term_forget_shrine_sealed_cards_hint;
		confirmText.text = CellUI<CharacterClassState>.textData.term_forget_shrine_unapplied_changes_confirm;
	}

	protected override void OnSetup(CharacterClassState setupData)
	{
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<CharacterClassState>.applicationState, "神像间-遗忘界面", characterClassState.classDef.FullID);
		CellUI<CharacterClassState>.fortressState.uiManager.fortressGuidePanel.forgetShrineGuideHint.TryFirstOpenAutoPopup();
		base.gameObject.SetActive(value: true);
		hasChanges = false;
		sequenceChanged = false;
		List<ItemDefinition> forgetShrineItems = characterClassState.GetForgetShrineItems();
		List<ItemDefinition> displayableItems = GetDisplayableItems();
		shrineItems.Clear();
		deckItems.Clear();
		foreach (ItemDefinition item in displayableItems)
		{
			if (forgetShrineItems.Contains(item))
			{
				shrineItems.Add(new Item(item));
			}
			else
			{
				deckItems.Add(new Item(item));
			}
		}
		cardDeckTitle.text = CellUI<CharacterClassState>.textData.term_items.FormatWith(characterClassState.classDef.characterName);
		forgetShrinePointTitle.text = CellUI<CharacterClassState>.textData.term_forget_wax_seal_count.FormatWith(ForgetShrinePointReward.GetName(characterClassState.classDef), characterClassState.forgetShrinePoint);
		if (forgetShrineIcon != null)
		{
			forgetShrineIcon.sprite = ((characterClassState.classDef.forgetShrinePointIcon != null) ? characterClassState.classDef.forgetShrinePointIcon : ScriptableObjectSingleton<GameDataManager>.instance.forgetShrinePointIcon);
		}
		deckSlots.Clear();
		shrineSlots.Clear();
		int count = displayableItems.Count;
		for (int i = 0; i < count; i++)
		{
			deckSlots.Add(new ForgetShrineItemSlot.Data
			{
				index = i,
				type = ForgetShrineItemSlot.EType.Deck
			});
		}
		for (int j = 0; j < forgetShrinePoint; j++)
		{
			shrineSlots.Add(new ForgetShrineItemSlot.Data
			{
				index = j,
				type = ForgetShrineItemSlot.EType.ForgetShrine
			});
		}
		itemDisplayPool.DespawnAll();
		deckItemSlotPool.DespawnAll();
		shrineItemSlotPool.DespawnAll();
		deckItemSlotPool.pool.UpdateSequenceDisplay(deckSlots, deckLayoutGroup);
		shrineItemSlotPool.pool.UpdateSequenceDisplay(shrineSlots, shrineLayoutGroup);
		SetForgetShineRectImage(0f);
		SetDeckRectImage(0f);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		pointText.text = $"{shrineItems.Count}/{characterClassState.forgetShrinePoint}";
		if (sequenceChanged)
		{
			sequenceChanged = false;
			OnSequenceChange();
		}
	}

	public ForgetShrineItemSlot GetSlot(ForgetShrineItemSlot.EType type, int index)
	{
		TraceLog.Debug($"[ForgetShrinePanel] GetSlot({type}, {index})");
		if (index < 0)
		{
			return null;
		}
		switch (type)
		{
		case ForgetShrineItemSlot.EType.Deck:
			if (index >= deckItems.Count)
			{
				return null;
			}
			return deckItemSlotPool.pool.GetActiveElement(deckSlots[index]);
		case ForgetShrineItemSlot.EType.ForgetShrine:
			if (index >= shrineItems.Count)
			{
				return null;
			}
			return shrineItemSlotPool.pool.GetActiveElement(shrineSlots[index]);
		default:
			return null;
		}
	}

	public ItemDisplayBase GetItemDisplay(ForgetShrineItemSlot.EType type, int index)
	{
		TraceLog.Debug($"[ForgetShrinePanel] GetItemDisplay({type}, {index})");
		if (index < 0)
		{
			return null;
		}
		switch (type)
		{
		case ForgetShrineItemSlot.EType.Deck:
			if (index >= deckItems.Count)
			{
				return null;
			}
			return itemDisplayPool.pool.GetActiveElement(deckItems[index]);
		case ForgetShrineItemSlot.EType.ForgetShrine:
			if (index >= shrineItems.Count)
			{
				return null;
			}
			return itemDisplayPool.pool.GetActiveElement(shrineItems[index]);
		default:
			return null;
		}
	}

	public Item GetItem(ForgetShrineItemSlot.EType type, int index)
	{
		if (index < 0)
		{
			return null;
		}
		switch (type)
		{
		case ForgetShrineItemSlot.EType.Deck:
			if (index >= deckItems.Count)
			{
				return null;
			}
			return deckItems[index];
		case ForgetShrineItemSlot.EType.ForgetShrine:
			if (index >= shrineItems.Count)
			{
				return null;
			}
			return shrineItems[index];
		default:
			return null;
		}
	}

	public bool CanMoveToShrine(Item item)
	{
		if (deckItems.Contains(item))
		{
			return forgetShrinePoint - shrineItems.Count > 0;
		}
		return false;
	}

	public void MoveToShrine(Item item)
	{
		if (CanMoveToShrine(item))
		{
			TraceLog.Debug("[ForgetShrinePanel] MoveToShrine(" + item.def.itemName + ")");
			deckItems.Remove(item);
			shrineItems.Add(item);
			SortSlots();
			hasChanges = true;
			sequenceChanged = true;
			SetDirty();
		}
	}

	public bool CanMoveToDeck(Item item)
	{
		if (shrineItems.Contains(item))
		{
			return true;
		}
		return false;
	}

	public void MoveToDeck(Item item)
	{
		if (CanMoveToDeck(item))
		{
			TraceLog.Debug("[ForgetShrinePanel] MoveToDeck(" + item.def.itemName + ")");
			shrineItems.Remove(item);
			deckItems.Add(item);
			SortSlots();
			hasChanges = true;
			sequenceChanged = true;
			SetDirty();
		}
	}

	public void OnSequenceChange()
	{
		SortSlots();
		foreach (ItemDisplayBase activeElement in itemDisplayPool.pool.GetActiveElements())
		{
			activeElement.GetModule<ForgetShrineModule>().OnSequenceChange();
		}
		foreach (ForgetShrineItemSlot activeElement2 in deckItemSlotPool.pool.GetActiveElements())
		{
			activeElement2.RefreshItem();
		}
		foreach (ForgetShrineItemSlot activeElement3 in shrineItemSlotPool.pool.GetActiveElements())
		{
			activeElement3.RefreshItem();
		}
		foreach (ItemDisplayBase item in itemDisplayPool.pool.GetActiveElements().ToList())
		{
			item.GetModule<ForgetShrineModule>().OnPostSequenceChange();
		}
	}

	public void SetDeckRectImage(float fade)
	{
		deckRectImage.SetA(fade);
	}

	public void SetForgetShineRectImage(float fade)
	{
		forgetShineRectImage.SetA(fade);
	}

	public void ShowQuickHint(string message)
	{
		if (CellUI<CharacterClassState>.applicationState != null && CellUI<CharacterClassState>.applicationState.fortressState != null)
		{
			FortressUIManager uiManager = CellUI<CharacterClassState>.applicationState.fortressState.uiManager;
			if (uiManager != null && uiManager.quickHintPanel != null)
			{
				uiManager.quickHintPanel.HintText(message);
			}
		}
	}

	private List<ItemDefinition> GetDisplayableItems()
	{
		HashSet<ItemDefinition> poolItemSet = new HashSet<ItemDefinition>(characterClassState.classDef.GetPoolItems(CellUI<CharacterClassState>.applicationState));
		return CellUI<CharacterClassState>.applicationState.assetCatalog.GetCharacterItems(currentClassType).FindAll((ItemDefinition item) => poolItemSet.Contains(item) && (CellUI<CharacterClassState>.applicationState.discoverManager?.IsDiscovered(item) ?? false));
	}

	private void OnApplyButtonClick()
	{
		ShowQuickHint(CellUI<CharacterClassState>.textData.term_saved);
		if (hasChanges)
		{
			characterClassState.SetForgetShrineItems(shrineItems.Select((Item i) => i.def).ToList());
		}
	}

	private void SortSlots()
	{
		deckItems.Sort((Item i, Item i2) => ((ICTAsset)i.def).AssetIndex.CompareTo(((ICTAsset)i2.def).AssetIndex));
		shrineItems.Sort((Item i, Item i2) => ((ICTAsset)i.def).AssetIndex.CompareTo(((ICTAsset)i2.def).AssetIndex));
	}
}
