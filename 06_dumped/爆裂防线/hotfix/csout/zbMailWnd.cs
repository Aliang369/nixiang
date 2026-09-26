using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMailWnd : zbTabWnd
{
	public VisualTreeAsset m_itemTA;

	private List<MailDetail> m_sortedMailList;

	private Button m_btnClose;

	private VisualElement m_listNo;

	private Button m_btn_get;

	private Button m_btn_del;

	private Button m_btnSupport;

	private Label m_lbcount;

	private readonly ListView[] m_tabLayers = (ListView[])(object)new ListView[2];

	private readonly Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbNormalMailTab m_panelNormal;

	private zbStandardMailTab m_panelStandard;

	private VisualElement m_notifyNormal;

	private VisualElement m_notifyStandard;

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	private List<Button> m_btns;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_mail.uxml");
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "close", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_btn_get = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btn_del = UQueryExtensions.Q<Button>(m_RootElement, "btn_del", (string)null);
		m_lbcount = UQueryExtensions.Q<Label>(m_RootElement, "lbcount", (string)null);
		m_btnSupport = UQueryExtensions.Q<Button>(m_RootElement, "btn_support", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<ListView>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_tabLayers[i], (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_tabLayers[i], (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
			((BaseVerticalCollectionView)m_tabLayers[i]).fixedItemHeight = 130f;
			((BaseVerticalCollectionView)m_tabLayers[i]).virtualizationMethod = (CollectionVirtualizationMethod)0;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_tabLayers[i], (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		}
		m_notifyNormal = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0]);
		m_notifyStandard = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1]);
		m_btns = new List<Button> { m_btn_get, m_btn_del };
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_get, (Action)OnGetAllAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_del, (Action)OnDeleteAll, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSupport, (Action<ClickEvent>)OnSupport, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Mail, OnEventUpdateMail);
		Button[] tabButtons = m_tabButtons;
		VisualElement[] tabLayers = (VisualElement[])(object)m_tabLayers;
		InitTab(tabButtons, tabLayers, OnSelect);
	}

	private void OnEventUpdateMail(object data)
	{
		if (IsShow())
		{
			SelectTab(m_currTab);
		}
	}

	public override void DoSelectTab()
	{
		m_sortedMailList = zbSelf.Instance.mailHandler.GetSortedMailList(m_currTab);
		switch (m_currTab)
		{
		case 0:
			if (m_panelNormal == null)
			{
				m_panelNormal = new zbNormalMailTab(m_tabLayers[0], this);
			}
			m_panelNormal.Refresh(m_sortedMailList);
			break;
		case 1:
			if (m_panelStandard == null)
			{
				m_panelStandard = new zbStandardMailTab(m_tabLayers[1], this);
			}
			m_panelStandard.Refresh(m_sortedMailList);
			break;
		}
		RefreshMailNumber();
	}

	public void RefreshMailNumber()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_sortedMailList.Count > 0;
		((TextElement)m_lbcount).text = m_sortedMailList.Count + "/" + zbSelf.Instance.mailHandler.GetUnReadCount(m_currTab);
		((VisualElement)m_tabLayers[m_currTab]).visible = flag;
		m_listNo.visible = !flag;
		((VisualElement)m_btn_get).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab != 0) ? 1 : 0));
	}

	public void OnGetAllAward()
	{
		if (zbSelf.Instance.mailHandler.HasAward())
		{
			zbWsMsgHandler.Instance.Mail_GetAllAward_Req();
		}
	}

	public void OnDeleteAll()
	{
		long[] array = zbSelf.Instance.mailHandler.CanDeleteIdList();
		if (array.Length != 0)
		{
			zbWsMsgHandler.Instance.Mail_Delete_Req(array);
		}
	}

	private void OnSupport(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	protected override void UpdatePerSecond()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		if (((BaseVerticalCollectionView)m_tabLayers[m_currTab]).itemsSource != null)
		{
			for (int i = 0; i < ((BaseVerticalCollectionView)m_tabLayers[m_currTab]).itemsSource.Count; i++)
			{
				if (((BaseVerticalCollectionView)m_tabLayers[m_currTab]).GetRootElementForIndex(i) is zbMailItem zbMailItem2)
				{
					zbMailItem2.UpdatePerSecond();
				}
			}
		}
		m_notifyNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.mailHandler.HasAward(0) && !zbSelf.Instance.mailHandler.HasUnRead(0)) ? 1 : 0));
		m_notifyStandard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.mailHandler.HasAward(1) && !zbSelf.Instance.mailHandler.HasUnRead(1)) ? 1 : 0));
	}

	private void OnSelect(int index)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			bool flag = i == index;
			if (flag)
			{
				((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(m_selectedIcon);
			}
			else
			{
				IStyle style = ((VisualElement)m_tabButtons[i]).style;
				StyleBackground backgroundImage = default(StyleBackground);
				((StyleBackground)(ref backgroundImage)).keyword = (StyleKeyword)3;
				style.backgroundImage = backgroundImage;
			}
			VisualElement val = ((i == 0) ? m_notifyNormal : m_notifyStandard);
			if (val != null)
			{
				val.transform.position = Vector2.op_Implicit(flag ? new Vector2(0f, 10f) : new Vector2(-8f, 10f));
			}
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public override void Refresh()
	{
		base.Refresh();
		List<MailDetail> sortedMailList = zbSelf.Instance.mailHandler.GetSortedMailList(1);
		if (sortedMailList == null || sortedMailList.Count == 0)
		{
			SelectTab(0);
		}
		else if (sortedMailList.Any((MailDetail mail) => mail.AwardState == EBoolState.False))
		{
			SelectTab(1);
		}
		else
		{
			SelectTab(0);
		}
		UpdatePerSecond();
	}
}
