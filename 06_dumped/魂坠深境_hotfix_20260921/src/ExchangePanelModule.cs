using TMPro;
using UnityEngine;

public class ExchangePanelModule : GlobalItemDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI reserveInfoText;

	[SerializeField]
	private TextMeshProUGUI countText;

	public override void UpdateDisplay()
	{
		int? globalItemStorageCount = base.globalItem.GetGlobalItemStorageCount(UIDisplayModule.applicationState);
		if (!globalItemStorageCount.HasValue)
		{
			reserveInfoText.gameObject.SetActive(value: false);
		}
		else
		{
			reserveInfoText.gameObject.SetActive(value: true);
			reserveInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_has.FormatWith(globalItemStorageCount);
		}
		int? num = GetComponentInParent<GlobalRewardBundleExchangePanel>()?.count;
		if (!num.HasValue)
		{
			countText.gameObject.SetActive(value: false);
			return;
		}
		countText.gameObject.SetActive(value: true);
		countText.text = $"{num}";
	}
}
