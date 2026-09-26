using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChatWnd : zbTabWnd
{
	private Button m_btnClose;

	private Button m_btnSend;

	private zbChatInputPanel m_inputPnl;

	private Label m_lbTitle;

	private zbChatWorldPanel m_chatWorldPnl;

	private zbChatLegionPanel m_chatLegionPnl;

	private zbChatSystemPanel m_chatSystemPnl;

	private zbChatFacePanel m_facePnl;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement m_faceMask;

	private VisualElement m_msgRemind;

	private Button m_btnMsgRemind;

	private Button m_btnCommunity;

	private Sprite m_select;

	private Sprite m_unSelect;

	private int m_unreadCount;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_faceMask = UQueryExtensions.Q<VisualElement>(m_RootElement, "face_mask", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnSend = UQueryExtensions.Q<Button>(m_RootElement, "btnsend", (string)null);
		m_msgRemind = UQueryExtensions.Q<VisualElement>(m_RootElement, "msgRemind", (string)null);
		m_btnMsgRemind = UQueryExtensions.Q<Button>(m_RootElement, "btn_msgRemind", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabButtons[i] = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(m_RootElement, "tab_container", (string)null), "btn_tab" + i, (string)null);
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(UQueryExtensions.Q<VisualElement>(m_RootElement, "layerContainer", (string)null), "layer" + i, (string)null);
		}
		m_inputPnl = new zbChatInputPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "pnl_input", (string)null), this);
		m_select = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x90.png");
		m_unSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x90_2.png");
		m_btnCommunity = UQueryExtensions.Q<Button>(m_RootElement, "btn_community", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCommunity, (Action<ClickEvent>)OnCommunity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_faceMask, (Action)HideFace, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMsgRemind, (Action)OnMsgRemind, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, OnSelect, m_currTab);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Chat_Init, OnInitEventChat);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Chat_Update, OnUpdateEventChat);
	}

	public override void Refresh()
	{
		base.Refresh();
		DoSelectTab();
	}

	protected void Update()
	{
		ChatState[] stateList = zbSelf.Instance.chatHandler.stateList;
		if (stateList == null)
		{
			return;
		}
		long num = 0L;
		switch (m_currTab)
		{
		case 0:
			stateList.FirstOrDefault((ChatState r) => r.ChatType == EChatType.Eworld);
			num = GetChatCd(EChatType.Eworld);
			break;
		case 1:
			stateList.FirstOrDefault((ChatState r) => r.ChatType == EChatType.Elegion);
			num = GetChatCd(EChatType.Elegion);
			break;
		}
		if (num > 0)
		{
			((VisualElement)m_btnSend).SetEnabled(false);
			((TextElement)m_btnSend).text = num.ToString();
		}
		else
		{
			((VisualElement)m_btnSend).SetEnabled(true);
			((TextElement)m_btnSend).text = zbUtil.Language("lang.10337");
		}
	}

	public static long GetChatCd(EChatType chatType)
	{
		ChatState chatState = zbSelf.Instance.chatHandler.stateList?.FirstOrDefault((ChatState s) => s.ChatType == chatType);
		if (chatState == null || chatState.CD <= 0)
		{
			return 0L;
		}
		long num = zbSelf.Instance.timeHandler.GetServerTime() - chatState.CD;
		if (num < 0)
		{
			num = 0L;
		}
		long num2 = 30 - num;
		if (num2 <= 0)
		{
			return 0L;
		}
		return num2;
	}

	public void OnSend(string item)
	{
		m_faceMask.visible = false;
		long num = 0L;
		if (m_currTab == 0)
		{
			num = GetChatCd(EChatType.Eworld);
			if (num > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10376"), num));
			}
			else if (m_chatWorldPnl != null)
			{
				m_chatWorldPnl.OnNewWorldMsg(item);
			}
		}
		else if (m_currTab == 1)
		{
			num = GetChatCd(EChatType.Elegion);
			if (num > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10376"), num));
			}
			else if (m_chatLegionPnl != null)
			{
				m_chatLegionPnl.OnNewLegionMsg(item);
			}
		}
	}

	private void OnSelect(int index)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((index == i) ? m_select : m_unSelect);
		}
	}

	public void ShowFace()
	{
		if (m_facePnl == null)
		{
			m_facePnl = new zbChatFacePanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "pnl_face", (string)null), this);
			m_facePnl.Init();
		}
		m_facePnl.Show();
		m_faceMask.visible = true;
	}

	public void HideFace()
	{
		if (m_facePnl != null)
		{
			m_facePnl.Hide();
		}
		m_faceMask.visible = false;
	}

	public override void DoSelectTab()
	{
		bool visible = m_currTab != 2;
		m_inputPnl.SetVisible(visible);
		if (m_currTab == 0)
		{
			if (m_chatWorldPnl == null)
			{
				m_chatWorldPnl = new zbChatWorldPanel(m_tabLayers[0], this);
			}
			SetTitle(zbUtil.Language("lang.10675"));
			zbWsMsgHandler.Instance.Chat_Data_Req(EChatType.Eworld, 0);
		}
		else if (m_currTab == 1)
		{
			if (m_chatLegionPnl == null)
			{
				m_chatLegionPnl = new zbChatLegionPanel(m_tabLayers[1], this);
			}
			SetTitle(zbUtil.Language("lang.10467"));
			zbWsMsgHandler.Instance.Chat_Data_Req(EChatType.Elegion, 0);
		}
		else if (m_currTab == 2)
		{
			if (m_chatSystemPnl == null)
			{
				m_chatSystemPnl = new zbChatSystemPanel(m_tabLayers[2], this);
			}
			SetTitle(zbUtil.Language("lang.10468"));
			m_chatSystemPnl.Refresh();
		}
		else
		{
			_ = m_currTab;
			_ = 3;
		}
	}

	private void OnMsgRemind()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (m_currTab == 0 && m_chatWorldPnl != null)
		{
			m_chatWorldPnl.UpdateViewList(scrollBottom: true);
		}
		else if (m_currTab == 1 && m_chatLegionPnl != null)
		{
			m_chatLegionPnl.UpdateViewList(scrollBottom: true);
		}
		m_msgRemind.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_unreadCount = 0;
	}

	public override bool IsCanOpen(int tabIndex)
	{
		return tabIndex != 3;
	}

	private void OnUpdateEventChat(object data)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		ScrollView val = null;
		float num = 130f;
		Action<bool> action = null;
		switch (m_currTab)
		{
		default:
			return;
		case 0:
			if (m_chatWorldPnl == null)
			{
				return;
			}
			val = m_chatWorldPnl.m_scrollView;
			num = m_chatWorldPnl.itemHeight;
			action = m_chatWorldPnl.UpdateViewList;
			break;
		case 1:
			if (m_chatLegionPnl == null)
			{
				return;
			}
			val = m_chatLegionPnl.m_scrollView;
			num = m_chatLegionPnl.itemHeight;
			action = m_chatLegionPnl.UpdateViewList;
			break;
		case 2:
			m_chatSystemPnl?.Refresh();
			return;
		}
		if (val != null)
		{
			float y = val.scrollOffset.y;
			Rect layout = ((VisualElement)val).contentContainer.layout;
			float height = ((Rect)(ref layout)).height;
			layout = ((VisualElement)val).layout;
			float height2 = ((Rect)(ref layout)).height;
			bool flag = y >= height - height2 - num;
			action?.Invoke(flag);
			if (flag)
			{
				m_unreadCount = 0;
				m_msgRemind.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_unreadCount++;
				m_msgRemind.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_btnMsgRemind).text = string.Format(zbUtil.Language("lang.10815"), m_unreadCount);
			}
		}
	}

	private void OnInitEventChat(object data)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (m_currTab == 0)
		{
			if (m_chatWorldPnl != null)
			{
				m_chatWorldPnl.UpdateViewList(scrollBottom: true);
				m_msgRemind.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_unreadCount = 0;
			}
		}
		else if (m_currTab == 1 && m_chatLegionPnl != null)
		{
			m_chatLegionPnl.UpdateViewList(scrollBottom: true);
		}
	}

	private void OnCommunity(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHub(1);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void SetTitle(string title)
	{
		((TextElement)m_lbTitle).text = title;
	}
}
