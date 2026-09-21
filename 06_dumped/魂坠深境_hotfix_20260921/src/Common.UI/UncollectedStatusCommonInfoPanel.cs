using System;

namespace Common.UI;

public class UncollectedStatusCommonInfoPanel : ICommonInfoPanel
{
	private readonly Func<ICommonInfoPanel> getTargetPanel;

	public UncollectedStatusCommonInfoPanel(Func<ICommonInfoPanel> getTargetPanel)
	{
		this.getTargetPanel = getTargetPanel;
	}

	public void ShowCommonInfo(CommonInfoData info)
	{
		if (info != null)
		{
			info.showUncollectedStatus = true;
		}
		getTargetPanel?.Invoke()?.ShowCommonInfo(info);
	}

	public void HideCommonInfo(object obj, CommonInfoData.ControlType controlType)
	{
		getTargetPanel?.Invoke()?.HideCommonInfo(obj, controlType);
	}
}
