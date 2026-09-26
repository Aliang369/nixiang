using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMailDetailWnd : zbBaseWnd
{
	private MailDetail m_currMailDetail;

	private MailContent m_currMailContent;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbContent;

	private Label m_lbTime;

	private Button m_Button_goto;

	private Button m_Button_delete;

	private Button m_Button_onGet;

	private VisualElement m_awardList;

	private zbItem[] m_items = new zbItem[5];

	private string m_mailContentStr;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lbTitle", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>(m_RootElement, "lb_content", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lbTime", (string)null);
		m_Button_goto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goTo", (string)null);
		m_awardList = UQueryExtensions.Q<VisualElement>(m_RootElement, "AwardList", (string)null);
		m_Button_delete = UQueryExtensions.Q<Button>(m_RootElement, "Button_delete", (string)null);
		m_Button_onGet = UQueryExtensions.Q<Button>(m_RootElement, "Button_onGet", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_delete, (Action)OnDelete, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_onGet, (Action<ClickEvent>)onGetAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_goto, (Action<ClickEvent>)OnOpenURL, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Mail, OnEventUpdateMail);
	}

	public void RefreshCustom(MailDetail mailDetail, MailContent mailContent)
	{
		m_currMailDetail = mailDetail;
		m_currMailContent = mailContent;
		((TextElement)m_lbTitle).text = zbUtil.Language(m_currMailDetail.Title);
		m_mailContentStr = zbUtil.GetContentString(m_currMailContent.Content);
		((TextElement)m_lbContent).text = m_mailContentStr;
		foreach (MailDetail mail in zbSelf.Instance.mailHandler.mailList)
		{
			if (mail.MailId == m_currMailDetail.MailId)
			{
				((VisualElement)m_Button_delete).visible = mail.AwardState == EBoolState.True;
				((VisualElement)m_Button_onGet).visible = mail.AwardState == EBoolState.False && mail.AwardList.Count > 0;
			}
		}
		RefreshAward();
		RefreshLinkButton();
		UpdatePerSecond();
	}

	protected override void UpdatePerSecond()
	{
		((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(m_currMailDetail.ExpireTime - zbSelf.Instance.timeHandler.GetServerTime());
	}

	private void RefreshLinkButton()
	{
		((VisualElement)m_Button_goto).visible = !string.IsNullOrEmpty(m_currMailContent.Link);
	}

	public void RefreshAward()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		m_awardList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_currMailDetail.AwardList.Count == 0));
		if (m_currMailDetail.AwardList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_items[i], "spriteGetAward", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail award in m_currMailDetail.AwardList)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
				if (m_currMailDetail.AwardState == EBoolState.True)
				{
					UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_items[num], "spriteGetAward", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
			num++;
		}
	}

	public void OnDelete()
	{
		long[] idList = new long[1] { m_currMailDetail.MailId };
		zbWsMsgHandler.Instance.Mail_Delete_Req(idList);
		Hide();
	}

	public void onGetAward(object onj)
	{
		zbWsMsgHandler.Instance.Mail_GetAward_Req(m_currMailDetail.MailId);
	}

	private void OnOpenURL(ClickEvent evt)
	{
		zbUtil.LinkToURLWithoutParams(m_currMailContent.Link);
	}

	private void OnEventUpdateMail(object obj)
	{
		if (m_currMailDetail != null && m_currMailContent != null)
		{
			MailDetail mailDetail = zbSelf.Instance.mailHandler.GetMailDetail(m_currMailDetail.MailId);
			if (mailDetail == null)
			{
				Hide();
			}
			else
			{
				RefreshCustom(mailDetail, m_currMailContent);
			}
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
