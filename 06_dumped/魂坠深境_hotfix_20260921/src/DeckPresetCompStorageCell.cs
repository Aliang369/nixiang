using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckPresetCompStorageCell : CellUI<DeckPresetCompDef>, IBeginDragHandler, IEventSystemHandler, IDragHandler
{
	public GameObject backGlow;

	public RuneDisplay runeDisplay;

	public ItemDisplayBase itemDisplay;

	public InfluenceDisplay influenceDisplay;

	public TextMeshProUGUI compNameText;

	public TextMeshProUGUI diyPointText;

	public TextMeshProUGUI stackCountText;

	public Image topMask;

	public Button clickButton;

	[NonSerialized]
	private DeckPresetEditPanel parentDeckPresetEditPanel;

	protected override void Init()
	{
		InitUI(runeDisplay);
		InitUI(itemDisplay);
		InitUI(influenceDisplay);
		parentDeckPresetEditPanel = GetUIInParents<DeckPresetEditPanel>();
		if (clickButton != null)
		{
			clickButton.onClick.AddListener(OnClick);
		}
	}

	private void OnClick()
	{
		if (base.setupData == null)
		{
			return;
		}
		ICommonInfoPanel commonInfoPanel = CellUI<DeckPresetCompDef>.fortressState?.uiManager?.universalCommonInfoPanel;
		if (commonInfoPanel != null)
		{
			object obj = base.setupData.GenerateTargetObject();
			if (obj != null)
			{
				CommonInfoData info = new CommonInfoData(obj, CommonInfoData.ControlType.Click, base.rectTransform, false);
				commonInfoPanel.ShowCommonInfo(info);
			}
		}
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData == null)
		{
			return;
		}
		runeDisplay.gameObject.SetActive(value: false);
		itemDisplay.gameObject.SetActive(value: false);
		influenceDisplay.gameObject.SetActive(value: false);
		DeckPresetComp deckPresetComp = new DeckPresetComp(base.setupData);
		if (base.setupData.type == DeckPresetCompType.Rune)
		{
			runeDisplay.gameObject.SetActive(value: true);
			runeDisplay.Setup(deckPresetComp.GenerateRune());
		}
		else if (base.setupData.type == DeckPresetCompType.Item)
		{
			itemDisplay.gameObject.SetActive(value: true);
			itemDisplay.Setup(deckPresetComp.GenerateItem());
		}
		else if (base.setupData.type == DeckPresetCompType.Influence)
		{
			influenceDisplay.gameObject.SetActive(value: true);
			influenceDisplay.Setup(deckPresetComp.GenerateInfluence());
		}
		compNameText.text = base.setupData.DisplayName;
		diyPointText.text = base.setupData.DIYPoint.ToString();
		if (parentDeckPresetEditPanel != null)
		{
			stackCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_multiply_X_arg0.FormatWith(parentDeckPresetEditPanel.GetRemainStackCount(base.setupData));
		}
		else
		{
			int num = CellUI<DeckPresetCompDef>.applicationState.pvpModeManager.storagePVPDeckPresetComps.FirstOrDefault((PVPModeManager.DeckPresetCompStorage s) => s.def == base.setupData)?.count ?? 0;
			stackCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_multiply_X_arg0.FormatWith(num);
		}
		topMask.gameObject.SetActive(value: false);
		backGlow.gameObject.SetActive(this == parentDeckPresetEditPanel?.draggingStorageCell);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (base.setupData != null && !(parentDeckPresetEditPanel == null) && this != parentDeckPresetEditPanel.draggingStorageCell)
		{
			parentDeckPresetEditPanel.draggingCompDef = base.setupData;
			parentDeckPresetEditPanel.draggingStorageCell.SetActiveAndDirty(active: true);
			parentDeckPresetEditPanel.draggingStorageCell.Setup(base.setupData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
}
