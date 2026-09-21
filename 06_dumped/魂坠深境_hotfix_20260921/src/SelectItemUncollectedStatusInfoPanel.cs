using System;
using Common.UI;

public class SelectItemUncollectedStatusInfoPanel : ICommonInfoPanel
{
	private readonly Func<ICommonInfoPanel> getDefaultPanel;

	private readonly Func<ICommonInfoPanel> getUncollectedPanel;

	private readonly Func<bool> shouldShowUncollectedStatus;

	public SelectItemUncollectedStatusInfoPanel(Func<ICommonInfoPanel> getDefaultPanel, Func<ICommonInfoPanel> getUncollectedPanel, Func<bool> shouldShowUncollectedStatus)
	{
		this.getDefaultPanel = getDefaultPanel;
		this.getUncollectedPanel = getUncollectedPanel;
		this.shouldShowUncollectedStatus = shouldShowUncollectedStatus;
	}

	public void ShowCommonInfo(CommonInfoData info)
	{
		GetPanel()?.ShowCommonInfo(info);
	}

	public void HideCommonInfo(object obj, CommonInfoData.ControlType controlType)
	{
		GetPanel()?.HideCommonInfo(obj, controlType);
	}

	private ICommonInfoPanel GetPanel()
	{
		Func<bool> func = shouldShowUncollectedStatus;
		if (func == null || !func())
		{
			return getDefaultPanel?.Invoke();
		}
		return getUncollectedPanel?.Invoke();
	}
}
