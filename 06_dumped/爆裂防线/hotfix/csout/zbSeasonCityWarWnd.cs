using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonCityWarWnd : zbBaseWnd
{
	private Button m_btnInfo;

	private Button m_btnTo;

	private Button m_btnSeasonShop;

	private Button m_btnTouch;

	private Label m_lbLeftTime;

	private Label m_lbLeftTimeVal;

	private VisualElement[] m_awards = (VisualElement[])(object)new VisualElement[6];

	private Button[] m_btnsBox = (Button[])(object)new Button[6];

	private VisualElement[] m_btnsLight = (VisualElement[])(object)new VisualElement[6];

	private zbItem[] m_cityAwards = new zbItem[5];

	private SeasonCityWarInfoAck m_cityWarInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnInfo = UQueryExtensions.Q<Button>(m_RootElement, "btn_info", (string)null);
		m_btnTo = UQueryExtensions.Q<Button>(m_RootElement, "btn_to", (string)null);
		m_btnSeasonShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_seasonShop", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_lbLeftTimeVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTimeVal", (string)null);
		UQueryExtensions.Q<ScrollView>(m_RootElement, "onlineAwardScrollView", (string)null);
		ScrollView val = UQueryExtensions.Q<ScrollView>(m_RootElement, "cityAwardScrollView", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_cityAwards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)val, $"item{i}", (string)null);
			m_cityAwards[i].OnEnable(this);
		}
		for (int j = 0; j < 6; j++)
		{
			m_awards[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "award" + j, (string)null);
			m_btnsBox[j] = UQueryExtensions.Q<Button>(m_awards[j], "btn_box", (string)null);
			m_btnsLight[j] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnsBox[j], "light", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInfo, (Action)OnInfo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTo, (Action)OnTo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSeasonShop, (Action)OnSeasonShop, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	protected override void Start()
	{
		base.Start();
	}

	public void RefreshCustom()
	{
		m_cityWarInfo = zbSelf.Instance.seasonHandler.seasonCityWarInfoAck;
		RefreshLeftTime();
		RefreshAward();
	}

	private void RefreshLeftTime()
	{
		bool flag = m_cityWarInfo.State == ESeasonCityWarState.Opening;
		((TextElement)m_lbLeftTime).text = zbUtil.Language(flag ? "lang.11486" : "lang.11485");
		if (flag)
		{
			((TextElement)m_lbLeftTimeVal).text = string.Format(zbUtil.Language("lang.11488"), zbUtil.FormatTimeHHMMSS(m_cityWarInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime()));
		}
		else
		{
			((TextElement)m_lbLeftTimeVal).text = string.Format(zbUtil.Language("lang.11487"), zbUtil.FormatTimeHHMMSS(m_cityWarInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime()));
		}
	}

	private void RefreshAward()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		List<List<int>> list = zbDataConfig.Instance.seasonBaseJsonTable.GetRecord()["firstCapRewardPreview"][(object)"value"].ToObject<List<List<int>>>();
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < list.Count;
			((VisualElement)m_cityAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)list[i][0], list[i][1], list[i][2]);
				m_cityAwards[i].Refresh(data, isNameVis: false, isCountVis: false);
			}
		}
	}

	private void OnInfo()
	{
	}

	private void OnTo()
	{
		OnClose();
	}

	private void OnSeasonShop()
	{
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshLeftTime();
	}

	public void OnClose()
	{
		Hide();
	}
}
