using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class DeckPresetEditPanel : UIBase
{
	public GameButton confirmButton;

	public GameButton cancelButton;

	public RectTransform editingCompHoverArea;

	public RectTransform storageCompHoverArea;

	public TextMeshProUGUI diyPointText;

	public TextMeshProUGUI deckCountText;

	public TMP_Dropdown storageTypeFilterDropDown;

	public TMP_InputField storageSearchInputField;

	public GameObject needDragCompHint;

	public DeckPresetCompStorageCell compStorageCellPrefab;

	public Transform compStorageContent;

	public DeckPresetCompCell deckPresetCompCellPrefab;

	public DeckPresetCompCell deckPresetSubCompCellPrefab;

	public RectTransform deckPresetCompCellParent;

	public PVPDefenderRuleTextDisplay ruleTextDisplay;

	public DeckPresetCompStorageCell draggingStorageCell;

	public DeckPresetCompCell draggingDeckPresetCompCell;

	[NonSerialized]
	public PVPDefender currentDefender;

	[NonSerialized]
	public DeckPresetCompDef draggingCompDef;

	[NonSerialized]
	public DeckPresetCompCell hoveringParentCompCell;

	[NonSerialized]
	public DeckPresetCompType storageTypeFilter;

	[NonSerialized]
	private PVPDefenderDetailPanel pvpDefenderDetailPanelParent;

	[NonSerialized]
	private CellUIPrefabPool<DeckPresetCompStorageCell, DeckPresetCompDef> compStorageCellPool;

	[NonSerialized]
	private CellUIPrefabPool<DeckPresetCompCell, DeckPresetComp> deckPresetCompCellPool;

	[NonSerialized]
	private CellUIPrefabPool<DeckPresetCompCell, DeckPresetComp> deckPresetSubCompCellPool;

	public DeckPreset currentDeckPreset;

	public static List<(DeckPresetCompType, string)> StorageTypeFilterOptions => new List<(DeckPresetCompType, string)>
	{
		(DeckPresetCompType.None, "--"),
		(DeckPresetCompType.Item, ScriptableObjectSingleton<TextDataPreset>.instance.term_item),
		(DeckPresetCompType.Rune, ScriptableObjectSingleton<TextDataPreset>.instance.term_rune),
		(DeckPresetCompType.Influence, ScriptableObjectSingleton<TextDataPreset>.instance.term_influence)
	};

	protected override void Init()
	{
		InitUI(draggingStorageCell);
		InitUI(draggingDeckPresetCompCell);
		InitUI(ruleTextDisplay);
		pvpDefenderDetailPanelParent = GetUIInParents<PVPDefenderDetailPanel>();
		compStorageCellPool = GetPrefabPool<DeckPresetCompStorageCell, DeckPresetCompDef>(compStorageCellPrefab, compStorageContent);
		deckPresetCompCellPool = GetPrefabPool<DeckPresetCompCell, DeckPresetComp>(deckPresetCompCellPrefab, deckPresetCompCellParent);
		deckPresetSubCompCellPool = GetPrefabPool<DeckPresetCompCell, DeckPresetComp>(deckPresetSubCompCellPrefab, deckPresetCompCellParent);
		draggingStorageCell.gameObject.SetActive(value: false);
		draggingDeckPresetCompCell.gameObject.SetActive(value: false);
		storageTypeFilterDropDown.options = StorageTypeFilterOptions.Select(((DeckPresetCompType, string) s) => new TMP_Dropdown.OptionData(s.Item2)).ToList();
		storageTypeFilterDropDown.onValueChanged.AddListener(_OnOptionFilterChanged);
		confirmButton.onClick.AddListener(delegate
		{
			currentDefender.deckPreset = currentDeckPreset;
			pvpDefenderDetailPanelParent.SetDirty();
			base.gameObject.SetActive(value: false);
		});
		confirmButton.onNotInteractableClick.AddListener(_OnNotInteractableClick);
		cancelButton.onClick.AddListener(delegate
		{
			pvpDefenderDetailPanelParent.SetDirty();
			base.gameObject.SetActive(value: false);
		});
		storageSearchInputField.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
	}

	private void Update()
	{
		if (draggingCompDef == null)
		{
			return;
		}
		if (draggingDeckPresetCompCell.isActiveAndEnabled)
		{
			hoveringParentCompCell = null;
			foreach (DeckPresetCompCell activeElement in deckPresetCompCellPool.GetActiveElements())
			{
				if (activeElement.setupData.def.type == DeckPresetCompType.Item && draggingCompDef.type == DeckPresetCompType.Rune && RectTransformUtility.RectangleContainsScreenPoint(activeElement.rectTransform, (Vector2)Input.mousePosition))
				{
					hoveringParentCompCell = activeElement;
				}
			}
		}
		if (Input.touchCount == 0 && !Input.GetMouseButton(0))
		{
			if (draggingDeckPresetCompCell.isActiveAndEnabled)
			{
				if (draggingCompDef.type == DeckPresetCompType.Rune)
				{
					if (hoveringParentCompCell != null)
					{
						hoveringParentCompCell.setupData.subComps.Add(new DeckPresetComp(draggingCompDef));
					}
					else
					{
						UIBase.applicationState.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_mode_need_drag_rune_comp_to_item);
					}
				}
				else
				{
					currentDeckPreset.comps.Add(new DeckPresetComp(draggingCompDef));
				}
			}
			draggingCompDef = null;
			hoveringParentCompCell = null;
			draggingStorageCell.SetActiveAndDirty(active: false);
			draggingDeckPresetCompCell.SetActiveAndDirty(active: false);
			SetDirty();
			return;
		}
		if (draggingStorageCell.isActiveAndEnabled)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(editingCompHoverArea, (Vector2)Input.mousePosition))
			{
				draggingStorageCell.SetActiveAndDirty(active: false);
				draggingDeckPresetCompCell.SetActiveAndDirty(active: true);
				draggingDeckPresetCompCell.Setup(new DeckPresetComp(draggingCompDef));
			}
		}
		else if (draggingDeckPresetCompCell.isActiveAndEnabled && RectTransformUtility.RectangleContainsScreenPoint(storageCompHoverArea, (Vector2)Input.mousePosition))
		{
			draggingDeckPresetCompCell.SetActiveAndDirty(active: false);
			draggingStorageCell.SetActiveAndDirty(active: true);
			draggingStorageCell.Setup(draggingDeckPresetCompCell.setupData.def);
		}
		draggingDeckPresetCompCell.rectTransform.position = Input.mousePosition;
		draggingStorageCell.rectTransform.position = Input.mousePosition;
	}

	public void Setup(PVPDefender defender)
	{
		currentDefender = defender;
		currentDeckPreset = defender.deckPreset.Copy();
		RefreshCompStorageCells();
		RefreshDeckPresetCompCells();
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (currentDeckPreset != null)
		{
			RefreshCompStorageCells();
			RefreshDeckPresetCompCells();
			PVPEnemyAvatarConfig enemyAvatarConfig = currentDefender.GetEnemyAvatarConfig(UIBase.applicationState.assetCatalog);
			PVPDefenderProperty pVPDefenderProperty = enemyAvatarConfig.GenerateBaseDefenderProperty(currentDefender.deckPreset);
			diyPointText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(currentDeckPreset.SumDiyPoint, pVPDefenderProperty.diyPoint);
			diyPointText.color = ((currentDeckPreset.SumDiyPoint > pVPDefenderProperty.diyPoint) ? Color.red : Color.white);
			deckCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(currentDeckPreset.DeckItemCount, pVPDefenderProperty.minimalDeckSize);
			deckCountText.color = ((currentDeckPreset.DeckItemCount < pVPDefenderProperty.minimalDeckSize) ? Color.red : Color.white);
			confirmButton.interactable = currentDeckPreset.IsLegalDeckPreset(enemyAvatarConfig);
			ruleTextDisplay.Setup(enemyAvatarConfig, currentDeckPreset);
			needDragCompHint.gameObject.SetActive(deckPresetCompCellPool.activeCount == 0);
		}
	}

	public void RefreshCompStorageCells()
	{
		List<DeckPresetCompDef> list = (from c in (from storage in UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps
				select storage.def into def
				where GetRemainStackCount(def) > 0
				where (storageTypeFilter == DeckPresetCompType.None || def.type == storageTypeFilter) && (string.IsNullOrEmpty(storageSearchInputField.text) || def.DisplayName.Contains(storageSearchInputField.text, StringComparison.OrdinalIgnoreCase))
				select def).ToList()
			orderby c.type, c.DIYPoint, c.item?.GetMajorTag().FullID, c.DisplayName
			select c).ToList();
		compStorageCellPool.DespawnAll();
		compStorageCellPool.Sync(list);
	}

	public void RefreshDeckPresetCompCells()
	{
		deckPresetCompCellPool.DespawnAll();
		deckPresetSubCompCellPool.DespawnAll();
		foreach (DeckPresetComp item in (from c in currentDeckPreset.comps
			orderby c.def.type, c.def.DIYPoint descending, c.def.DisplayName
			select c).ToList())
		{
			deckPresetCompCellPool.Spawn(item);
			if (item.subComps == null || item.subComps.Count <= 0)
			{
				continue;
			}
			foreach (DeckPresetComp subComp in item.subComps)
			{
				deckPresetSubCompCellPool.Spawn(subComp);
			}
		}
	}

	public int GetRemainStackCount(DeckPresetCompDef comp)
	{
		if (comp == null)
		{
			return 0;
		}
		int num = UIBase.applicationState.pvpModeManager.storagePVPDeckPresetComps.FirstOrDefault((PVPModeManager.DeckPresetCompStorage c) => c.def.UniqueID == comp.UniqueID)?.count ?? 0;
		int num2 = 0;
		if (currentDeckPreset != null)
		{
			num2 = currentDeckPreset.GetAllExpandedComps().Count((DeckPresetComp c) => c.def.UniqueID == comp.UniqueID);
		}
		return num - num2;
	}

	private void _OnNotInteractableClick()
	{
		if (!currentDeckPreset.IsItemCountLegal(currentDefender.GetEnemyAvatarConfig(UIBase.applicationState.assetCatalog)))
		{
			UIBase.applicationState.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_mode_item_count_less_than_minimal_size);
		}
		else if (!currentDeckPreset.IsDiyPointLegal(currentDefender.GetEnemyAvatarConfig(UIBase.applicationState.assetCatalog)))
		{
			UIBase.applicationState.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_mode_item_diy_point_greater_than_max);
		}
	}

	private void _OnOptionFilterChanged(int index)
	{
		if (index >= 0 && index < StorageTypeFilterOptions.Count)
		{
			storageTypeFilter = StorageTypeFilterOptions[index].Item1;
			SetDirty();
		}
	}
}
