using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image counterTypeImage;

	[SerializeField]
	private TextMeshProUGUI counterDisplayText;

	[SerializeField]
	private TextMeshProUGUI counterDisplayTextShadow;

	public override void UpdateDisplay()
	{
		if (base.slotDisplay.indexHide)
		{
			counterTypeImage.gameObject.SetActive(value: false);
			counterDisplayText.gameObject.SetActive(value: false);
			counterDisplayTextShadow?.gameObject.SetActive(value: false);
			return;
		}
		counterTypeImage.gameObject.SetActive(base.slotDisplay.item.withCounter);
		counterDisplayText.gameObject.SetActive(value: true);
		counterDisplayTextShadow?.gameObject.SetActive(value: true);
		counterDisplayText.text = base.slotDisplay.item.GetSpriteCounterText();
		counterDisplayText.color = base.slotDisplay.item.GetCounterTextColor();
		if (counterDisplayTextShadow != null)
		{
			counterDisplayTextShadow.text = counterDisplayText.text;
		}
		counterTypeImage.sprite = base.slotDisplay.item.GetCounterImage();
	}

	public void OnExile()
	{
		counterDisplayText.gameObject.SetActive(value: false);
		counterDisplayTextShadow?.gameObject.SetActive(value: false);
	}
}
