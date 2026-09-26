using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbNoticeItem : VisualElement
{
	private Label m_lblTitle;

	private Button m_btnTouch;

	private VisualElement m_newNotice;

	private VisualElement m_hotNotice;

	private NoticeDetail m_noticeDetail;

	private zbHubWnd m_wndParent;

	public zbNoticeItem(VisualTreeAsset tAsset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		((VisualElement)this).style.backgroundColor = new StyleColor(Color.clear);
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
	}

	public void Init(zbHubWnd wndParent, NoticeDetail info)
	{
		m_wndParent = wndParent;
		m_noticeDetail = info;
		m_lblTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "title", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_newNotice = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "newNotice", (string)null);
		m_hotNotice = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "hotNotice", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		RefreshNoticeDetail(m_noticeDetail);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void RefreshNoticeDetail(NoticeDetail noticeDetail)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		m_noticeDetail = noticeDetail;
		((TextElement)m_lblTitle).text = m_noticeDetail.Title;
		bool flag = zbNewNoticeHandler.IsNewNotice(m_noticeDetail.Id);
		m_newNotice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = !flag && m_noticeDetail.Status == ENoticeState.Recommend;
		m_hotNotice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_noticeDetail != null)
		{
			zbNewNoticeHandler.SetNoticeViewed(m_noticeDetail.Id);
		}
		m_wndParent.RefreshNew();
		RefreshNoticeDetail(m_noticeDetail);
		zbUIMgr.Instance.ShowNoticDetail(m_noticeDetail);
	}
}
