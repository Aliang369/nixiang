using System;
using System.Collections.Generic;
using Common.UI;

public abstract class CommonInfoPanelBase : UIBase, ICommonInfoPanel
{
	[NonSerialized]
	protected CommonInfoData currentCommonInfoData;

	[NonSerialized]
	protected List<CommonInfoData> infos;

	public bool HasCommonInfo
	{
		get
		{
			if (infos != null)
			{
				return infos.Count > 0;
			}
			return false;
		}
	}

	protected override void Init()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		infos = new List<CommonInfoData>();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		CommonInfoData shouldDisplayInfo = GetShouldDisplayInfo();
		if (currentCommonInfoData != shouldDisplayInfo)
		{
			currentCommonInfoData = shouldDisplayInfo;
			UpdateInfo();
		}
	}

	protected virtual void UpdateInfo()
	{
	}

	public void ShowCommonInfo(CommonInfoData info)
	{
		if (!PlatformHelper.IsMobile() || info.controlType != CommonInfoData.ControlType.Hover)
		{
			if (info.controlType == CommonInfoData.ControlType.Click)
			{
				infos.Clear();
			}
			if (infos.Find((CommonInfoData i) => i.target.Equals(info.target) && i.controlType == CommonInfoData.ControlType.Click) == null)
			{
				infos.Add(info);
				SetDirty();
			}
		}
	}

	public void HideCommonInfo(object obj, CommonInfoData.ControlType controlType)
	{
		CommonInfoData commonInfoData = infos.Find((CommonInfoData i) => i.target.Equals(obj) && i.controlType == controlType);
		if (commonInfoData != null)
		{
			infos.Remove(commonInfoData);
			SetDirty();
		}
	}

	public void HideAllCommonInfo()
	{
		if (infos != null)
		{
			infos.Clear();
			SetDirty();
		}
	}

	private CommonInfoData GetShouldDisplayInfo()
	{
		if (infos == null || infos.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < infos.Count; i++)
		{
			if (infos[i].controlType == CommonInfoData.ControlType.Click)
			{
				return infos[i];
			}
		}
		return infos[0];
	}

	private void _OnEmptyClicked()
	{
		HideAllCommonInfo();
	}
}
