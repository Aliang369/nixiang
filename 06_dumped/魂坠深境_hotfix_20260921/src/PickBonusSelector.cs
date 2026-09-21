using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickBonusSelector : CellUI<BattleBonusObject>, IDisplayModulesController
{
	[SerializeField]
	private Image InfoLabel;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private GameButton pickButton;

	[SerializeField]
	private GameButton pickButton2;

	[SerializeField]
	public ItemDisplayBase singleItemBonusDisplayBase;

	[NonSerialized]
	private PickBonusSelectionPanelBase attachPanel;

	[NonSerialized]
	public BattleBonusObject bonus;

	[NonSerialized]
	private RewardUncollectedTag itemUncollectedTag;

	protected override void Init()
	{
		InitUI(singleItemBonusDisplayBase);
		attachPanel = GetUIInParents<PickBonusSelectionPanelBase>();
		pickButton.onClick.AddListener(_OnPickButtonClicked);
		pickButton2.onClick.AddListener(_OnPickButtonClicked);
		singleItemBonusDisplayBase.GetModule<InfoModule>().infoPanel = attachPanel.itemInfoPanel;
		itemUncollectedTag = RewardUncollectedTag.AttachTo(singleItemBonusDisplayBase);
	}

	protected override void OnSetup(BattleBonusObject dailyContract)
	{
		bonus = dailyContract;
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		image.sprite = bonus.PreviewIcon;
		pickButton2.gameObject.SetActive(value: false);
		pickButton.gameObject.SetActive(value: false);
		if (bonus.isChoose)
		{
			pickButton2.gameObject.SetActive(value: true);
		}
		else
		{
			pickButton.gameObject.SetActive(value: true);
		}
		if (bonus.countText.IsNullOrEmpty())
		{
			countText.gameObject.SetActive(value: false);
		}
		else
		{
			countText.gameObject.SetActive(value: true);
			countText.text = bonus.countText;
		}
		if (bonus is GetSingleItemBonus getSingleItemBonus)
		{
			singleItemBonusDisplayBase.SetActiveAndDirty(active: true);
			image.gameObject.SetActive(value: false);
			singleItemBonusDisplayBase.Setup(getSingleItemBonus.item);
			itemUncollectedTag?.Setup(getSingleItemBonus.item?.GetDef(), CellUI<BattleBonusObject>.applicationState);
			InfoLabel.gameObject.SetActive(value: true);
			text.text = bonus.previewText;
		}
		else
		{
			singleItemBonusDisplayBase.SetActiveAndDirty(active: false);
			itemUncollectedTag?.Hide();
			image.gameObject.SetActive(value: true);
			InfoLabel.gameObject.SetActive(value: false);
			text.text = bonus.previewText;
		}
	}

	private void _OnPickButtonClicked()
	{
		attachPanel.Select(this);
	}

	private void Set(BattleBonusObject obj)
	{
		Setup(obj);
	}
}
