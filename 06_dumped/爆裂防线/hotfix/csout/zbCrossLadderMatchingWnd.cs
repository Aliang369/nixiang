using System;
using UnityEngine.UIElements;

public class zbCrossLadderMatchingWnd : zbBaseWnd
{
	private Label m_lbTime;

	private Button m_btnCancel;

	private int m_matchTime;

	private int m_randTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnCancel, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_CrossLadder_CombatList_Ack, OnCancel);
	}

	public override void Refresh()
	{
		m_matchTime = 0;
		m_randTime = Rand.Range(3, 6);
		RefreshTime();
	}

	private void RefreshTime()
	{
		if (m_matchTime == m_randTime || m_matchTime == 10)
		{
			zbWsMsgHandler.Instance.CrossLadder_CombatList_Req();
		}
		if (m_matchTime >= 15)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10890"));
			OnCancel();
		}
		((TextElement)m_lbTime).text = zbUtil.Language("lang.10889") + ": " + zbUtil.FormatTimeMMSS(m_matchTime);
		m_matchTime++;
	}

	private void OnCancel(object obj = null)
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}
}
