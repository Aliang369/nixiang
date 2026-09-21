using UnityEngine;

public class CommonInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private UniversalCommonInfoCell commonInfoCell;

	protected override void Init()
	{
		base.Init();
		InitUI(commonInfoCell);
		commonInfoCell.Unset();
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData == null)
		{
			commonInfoCell.Unset();
		}
		else
		{
			commonInfoCell.Setup(currentCommonInfoData);
		}
	}
}
