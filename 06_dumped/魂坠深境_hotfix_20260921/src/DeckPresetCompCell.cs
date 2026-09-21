using Common.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckPresetCompCell : CellUI<DeckPresetComp>, IPointerClickHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler
{
	public Image compTypeImage;

	public TextMeshProUGUI compNameText;

	public Image compIconImage;

	public TextMeshProUGUI diyPointText;

	public Image topMask;

	private DeckPresetEditPanel parentDeckPresetEditPanel;

	protected override void Init()
	{
		parentDeckPresetEditPanel = GetUIInParents<DeckPresetEditPanel>();
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData != null)
		{
			compTypeImage.sprite = base.setupData.def.DisplayTypeIcon;
			compNameText.text = base.setupData.def.DisplayName;
			compIconImage.sprite = base.setupData.def.DisplayIcon;
			diyPointText.text = base.setupData.def.DIYPoint.ToString();
		}
	}

	private void Update()
	{
		topMask.gameObject.SetActive(this == parentDeckPresetEditPanel.hoveringParentCompCell);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (base.setupData != null && !(parentDeckPresetEditPanel == null) && this != parentDeckPresetEditPanel.draggingDeckPresetCompCell)
		{
			parentDeckPresetEditPanel.draggingCompDef = base.setupData.def;
			parentDeckPresetEditPanel.draggingDeckPresetCompCell.SetActiveAndDirty(active: true);
			parentDeckPresetEditPanel.draggingDeckPresetCompCell.Setup(base.setupData);
			base.gameObject.SetActive(value: false);
			parentDeckPresetEditPanel.currentDeckPreset.RemoveComp(base.setupData);
			parentDeckPresetEditPanel.RefreshDeckPresetCompCells();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (base.setupData == null)
		{
			return;
		}
		ICommonInfoPanel commonInfoPanel = CellUI<DeckPresetComp>.fortressState?.uiManager?.universalCommonInfoPanel;
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
}
