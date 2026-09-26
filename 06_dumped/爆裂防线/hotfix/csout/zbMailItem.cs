using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMailItem : zbViewBaseItem
{
	private MailDetail m_mailDetail;

	private zbMailWnd m_wndParent;

	private Label m_lbTitle;

	private Label m_lbSender;

	private Label m_lbTime;

	private Button m_btnTouch;

	private VisualElement m_mailStar;

	private VisualElement m_redPoint;

	public zbMailItem(zbMailWnd parent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = parent;
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "title", (string)null);
		m_lbSender = UQueryExtensions.Q<Label>((VisualElement)(object)this, "sender", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "time", (string)null);
		m_mailStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mailStar", (string)null);
		m_redPoint = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "redPoint", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(MailDetail mailDetail)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		m_mailDetail = mailDetail;
		m_mailStar.visible = mailDetail.MailLevel == EMailLevel.Important;
		((TextElement)m_lbTitle).text = zbUtil.Language(m_mailDetail.Title);
		((TextElement)m_lbSender).text = zbUtil.Language(m_mailDetail.SenderName);
		bool flag = (mailDetail.AwardState == EBoolState.False && mailDetail.AwardList.Count > 0) || mailDetail.ReadState == EBoolState.False;
		m_redPoint.visible = flag;
		if (!flag)
		{
			((VisualElement)m_btnTouch).style.backgroundColor = StyleColor.op_Implicit(new Color(0f, 0f, 0f, 0.45f));
		}
		else
		{
			((VisualElement)m_btnTouch).style.backgroundColor = StyleColor.op_Implicit(new Color(0f, 0f, 0f, 0f));
		}
		UpdatePerSecond();
	}

	public void UpdatePerSecond()
	{
		((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(m_mailDetail.ExpireTime - zbSelf.Instance.timeHandler.GetServerTime());
	}

	public void OnTouch(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Mail_Read_Req(m_mailDetail.MailId);
	}
}
