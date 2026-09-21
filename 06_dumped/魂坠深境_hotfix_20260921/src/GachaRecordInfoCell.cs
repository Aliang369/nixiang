using TMPro;
using UnityEngine;

public class GachaRecordInfoCell : CellUI<GachaHistoryInfo>
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI timeText;

	[SerializeField]
	private GameObject newMark;

	[SerializeField]
	private TextMeshProUGUI typeText;

	[SerializeField]
	private TextMeshProUGUI contentText;

	public GachaPoolState Pool => GetUIInParents<GachaRecordInfo>().Pool;

	protected override void OnSetup(GachaHistoryInfo info)
	{
		info.reward.SetupGachaDisplayContent(info.contentType, info.content, info.qualityLevel);
		nameText.text = (info.gachaPoolDef ?? Pool.def).poolDisplayName;
		timeText.text = info.date.ToLocalTime().ToString("yyyy-M-d H:mm");
		newMark.SetActive(info.isNew);
		typeText.text = info.reward.gachaTypeString;
		contentText.text = ((IGlobalItemDisplaySetup)info.reward).GlobalItemCellName.ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos[((IGlobalItemDisplaySetup)info.reward).GlobalItemQuality].qualityBaseColor);
	}
}
