using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeadWnd : zbTabWnd
{
	private readonly VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private readonly Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbTabHead m_panelHead;

	private zbTabHeadFrame m_panelHeadFrame;

	private Button m_Button_close;

	private Button m_Button_AllClose;

	private VisualElement m_notifyHeadFrame;

	private VisualElement m_notifyHead;

	private Sprite m_cachedBtnSprite;

	private Sprite m_cachedBtnSpriteSel;

	protected override void OnEnable()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_Button_close = UQueryExtensions.Q<Button>(m_RootElement, "Button_Close", (string)null);
		m_Button_AllClose = UQueryExtensions.Q<Button>(m_RootElement, "Button_AllClose", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
			if (i == 0)
			{
				m_notifyHead = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i], (Vector2?)new Vector2(0f, -5f), 1f, true);
			}
			if (i == 1)
			{
				m_notifyHeadFrame = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i], (Vector2?)new Vector2(0f, -5f), 1f, true);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_close, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_AllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Head_Data, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, RefreshNotify);
		m_cachedBtnSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_208x73_1.png");
		m_cachedBtnSpriteSel = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_208x73_0.png");
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
	}

	public override void DoSelectTab()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((i == m_currTab) ? m_cachedBtnSpriteSel : m_cachedBtnSprite);
		}
		switch (m_currTab)
		{
		case 0:
			if (m_panelHead == null)
			{
				m_panelHead = new zbTabHead(m_tabLayers[0], this);
			}
			m_panelHead.Refresh(m_panelHead);
			break;
		case 1:
			if (m_panelHeadFrame == null)
			{
				m_panelHeadFrame = new zbTabHeadFrame(m_tabLayers[1], this);
			}
			m_panelHeadFrame.Refresh(m_panelHeadFrame);
			break;
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public override void Refresh()
	{
		base.Refresh();
		SelectTab(0);
		UpdateNotifyHeadDisplay();
		UpdateNotifyHeadFrameDisplay();
	}

	public void RefreshNotify(object obj)
	{
		zbSelf.Instance.headHandler.localOwnedAvatars = zbSelf.Instance.headHandler.LoadLocalOwnedAvatars();
		zbSelf.Instance.headHandler.SyncAvatarsFromServer(((IEnumerable<int>)zbServerData.sGameData.HeadList).ToList());
		UpdateNotifyHeadDisplay();
		UpdateNotifyHeadFrameDisplay();
	}

	private void UpdateNotifyHeadDisplay()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (m_notifyHead != null)
		{
			m_notifyHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.headHandler.HasNewHead()));
		}
	}

	private void UpdateNotifyHeadFrameDisplay()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (m_notifyHeadFrame != null)
		{
			m_notifyHeadFrame.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.headHandler.HasHeadFrameFirstNotify()));
		}
	}
}
