using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YinYangInfoDisplay : CellUI<YinYangFeatureAbility.IYinYangInfoData>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private YinYangFeatureAbility.IYinYangInfoData info;

	[NonSerialized]
	private ItemInfoDisplayBase itemInfoDisplay;

	protected override void Init()
	{
		itemInfoDisplay = parent as ItemInfoDisplayBase;
	}

	protected override void OnSetup(YinYangFeatureAbility.IYinYangInfoData dailyContract)
	{
		info = dailyContract;
		icon.sprite = dailyContract.GetIcon();
		text.rectTransform.anchoredPosition = dailyContract.GetTextOffset_ItemInfoDisplay();
		text.fontSharedMaterial = dailyContract.GetMaterial();
		ForceUpdate();
	}

	protected override void OnUnset()
	{
		info = null;
		text.text = null;
		icon.sprite = null;
	}

	protected override void UpdateDisplay()
	{
		text.text = info.GetText(CellUI<YinYangFeatureAbility.IYinYangInfoData>.gameState, itemInfoDisplay.item, itemInfoDisplay.isBattleDisplay);
		descriptionText.text = info.GetDescriptionText(CellUI<YinYangFeatureAbility.IYinYangInfoData>.gameState, itemInfoDisplay.item, itemInfoDisplay.isBattleDisplay);
	}
}
