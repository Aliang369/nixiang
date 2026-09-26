using System;
using System.Text.RegularExpressions;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbNoticDetailWnd : zbBaseWnd
{
	private static readonly Regex ChannelLinkRegex = new Regex("<a\\b(?<attributes>[^>]*)>.*?</a>(?:\\s*&#x20;)?", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	private NoticeDetail m_noticDetail;

	private VisualElement m_ElementContent;

	private ScrollView m_ListContent;

	private Label m_lbContent;

	private Label m_lbTitle;

	private Button m_btnClose;

	private Button m_btnGoto;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "goTo", (string)null);
		m_ElementContent = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_ListContent = UQueryExtensions.Q<ScrollView>(m_ElementContent, "sc_content", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>((VisualElement)(object)m_ListContent, "lbl_content", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_ElementContent, "title", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnUrl, "", (TrickleDown)0);
	}

	private void OnUrl(ClickEvent evt)
	{
		if (!string.IsNullOrEmpty(m_noticDetail.Link))
		{
			zbUtil.LinkToURLWithParams(m_noticDetail.Link);
		}
	}

	public void RefreshCustom(NoticeDetail _noticDetail)
	{
		m_noticDetail = _noticDetail;
		string text = FilterChannelLinkContent(m_noticDetail.Content);
		((TextElement)m_lbContent).text = text;
		((TextElement)m_lbTitle).text = m_noticDetail.SubTitle;
		((VisualElement)m_btnGoto).visible = !string.IsNullOrEmpty(m_noticDetail.Link);
	}

	private static string FilterChannelLinkContent(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			return string.Empty;
		}
		if (Application.isEditor)
		{
			return content;
		}
		return ChannelLinkRegex.Replace(content, delegate(Match match)
		{
			string value = match.Groups["attributes"].Value;
			if (value.IndexOf("bbs.3839.com", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				if (zbLoginUtil.appChannel != EChannelType.Hykb)
				{
					return string.Empty;
				}
				return match.Value;
			}
			return (value.IndexOf("www.taptap.cn", StringComparison.OrdinalIgnoreCase) >= 0) ? ((zbLoginUtil.appChannel != EChannelType.TapTap && zbLoginUtil.appChannel != EChannelType.Ios && zbLoginUtil.appChannel != EChannelType.Dyunion) ? string.Empty : match.Value) : match.Value;
		});
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
