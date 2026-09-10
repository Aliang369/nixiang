using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonReference : CellUI<CommonInfoData>
{
	[SerializeField]
	private bool showIcon;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI iconStackText;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private RectTransform parentRectTransform;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<CommonInfoData>.applicationState, descriptionText, base.rectTransform);
		parentRectTransform = base.transform.parent as RectTransform;
	}

	protected override void OnSetup(CommonInfoData setupData)
	{
		base.gameObject.SetActive(value: true);
		icon.gameObject.SetActive(showIcon);
		UnityHelper.MoveTargetRectToNeighborRectInAreaRect(base.rectTransform, setupData.pivotRectTransform, parentRectTransform);
		SetDisplay(FeatureDisplay.GetDisplayInfo(setupData.target));
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	private void SetDisplay(FeatureDisplay.DisplayInfo displayInfo)
	{
		if (displayInfo == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		ResetBeforeSetup();
		if (displayInfo.icon != null)
		{
			icon.gameObject.SetActive(value: true);
			icon.sprite = displayInfo.icon;
		}
		nameText.text = displayInfo.name;
		descriptionText.text = displayInfo.description;
		if (!displayInfo.iconStackText.IsNullOrEmpty())
		{
			iconStackText.gameObject.SetActive(value: true);
			iconStackText.text = displayInfo.iconStackText;
		}
		textLinkObjectDisplayer.SetGenerator(displayInfo.generator);
		base.gameObject.SetActive(value: true);
	}

	private void ResetBeforeSetup()
	{
		icon.gameObject.SetActive(value: false);
		iconStackText.gameObject.SetActive(value: false);
	}
}
