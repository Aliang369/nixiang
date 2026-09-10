using UnityEngine;

namespace Common.UI;

public class CommonInfoData
{
	public enum ControlType
	{
		Click,
		Hover,
		FixDisplay
	}

	public ControlType controlType;

	public object target;

	public RectTransform pivotRectTransform;

	public bool showUncollectedStatus;

	public bool showHideButton;

	public (object key, object value)[] appendData;

	public CommonInfoData(object target, ControlType controlType = ControlType.Click, RectTransform pivotRectTransform = null, bool showUncollectedStatus = false, params (object key, object value)[] appendData)
	{
		this.target = target;
		this.controlType = controlType;
		this.pivotRectTransform = pivotRectTransform;
		this.showUncollectedStatus = showUncollectedStatus;
		this.appendData = appendData;
	}

	public override string ToString()
	{
		string text = "";
		try
		{
			text += $" target: {target}";
			text += $" controlType: {controlType}";
			text += $" pivotRectTransform: {pivotRectTransform}";
		}
		catch
		{
		}
		return text;
	}
}
