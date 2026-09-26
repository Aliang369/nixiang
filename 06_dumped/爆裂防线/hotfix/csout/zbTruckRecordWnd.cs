using System;
using UnityEngine.UIElements;

public class zbTruckRecordWnd : zbTabWnd
{
	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement m_spriteTabSel;

	private zbTruckRecordFight m_moduleRob;

	private zbTruckRecordTransport m_moduleSelf;

	private Label m_lbTitle;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			int idx = i;
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabButtons[i], (Action<ClickEvent>)delegate
			{
				onGoBind(idx);
			}, "", (TrickleDown)0);
		}
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		InitTab(m_tabButtons, m_tabLayers, null);
		((TextElement)m_tabButtons[0]).text = zbUtil.Language("lang.11145");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckSelfInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckMatchRefresh, OnEventUpdate);
	}

	private void OnEventUpdate(object data)
	{
		DoSelectTab();
	}

	public override void DoSelectTab()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if (m_moduleRob == null)
			{
				m_moduleRob = new zbTruckRecordFight();
				m_moduleRob.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleRob.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11145");
			break;
		case 1:
			if (m_moduleSelf == null)
			{
				m_moduleSelf = new zbTruckRecordTransport();
				m_moduleSelf.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleSelf.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			((TextElement)m_lbTitle).text = zbUtil.Language("发车记录");
			break;
		}
	}

	public void onGoBind(int tabIndex)
	{
		if (m_currTab != tabIndex)
		{
			if (tabIndex == 0)
			{
				zbWsMsgHandler.Instance.Truck_RobRecordList_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.Truck_TransportRecordList_Req();
			}
		}
	}

	public void RefreshCustom(int tabIndex)
	{
		SelectTab(tabIndex);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
