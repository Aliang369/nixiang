using CoreKit.Util;
using UnityEngine;

public class ChinaCompliancePanel : UIBase
{
	[SerializeField]
	private RectTransform ageHintPanel;

	[SerializeField]
	private GameButton ageHintCADPAButton;

	[SerializeField]
	private GameObject chinaComplianceInfoArea;

	[SerializeField]
	private GameObject healthyGameHintArea;

	[SerializeField]
	private GameButton closeAgeHintButton;

	protected override void Init()
	{
		ageHintCADPAButton.onClick.AddListener(_OnOpenAgeHintButton);
		closeAgeHintButton.onClick.AddListener(_OnCloseAgeHintButton);
		if (ShowCompliance())
		{
			ageHintCADPAButton.gameObject.SetActive(value: true);
			chinaComplianceInfoArea.gameObject.SetActive(value: true);
			healthyGameHintArea.SetActive(value: true);
		}
		else
		{
			ageHintCADPAButton.gameObject.SetActive(value: false);
			chinaComplianceInfoArea.gameObject.SetActive(value: false);
			healthyGameHintArea.SetActive(value: false);
		}
	}

	private bool ShowCompliance()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		if ((int)CSCommon.channelType == 3)
		{
			return AreaDetectUtil.IsChinaMainLand((CountryDetectionStrategy)2);
		}
		return (int)CSCommon.channelType != 2;
	}

	private void _OnOpenAgeHintButton()
	{
		ageHintPanel.gameObject.SetActive(value: true);
	}

	private void _OnCloseAgeHintButton()
	{
		ageHintPanel.gameObject.SetActive(value: false);
	}
}
