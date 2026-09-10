using UnityEngine;

public class ClavisInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private ClavisInfoDisplay clavisInfoDisplay;

	protected override void Init()
	{
		base.Init();
		InitUI(clavisInfoDisplay);
		clavisInfoDisplay.Unset();
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData == null)
		{
			clavisInfoDisplay.Unset();
		}
		else
		{
			clavisInfoDisplay.Setup(currentCommonInfoData.target as ClavisDef);
		}
	}
}
