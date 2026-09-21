using System;
using ChillyRoom.Announcement.V1;
using ChillyRoom.Utils.TMPExtension;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MainMenu;

public class AnnouncementRichTextContentView : AnnouncementContentView
{
	[SerializeField]
	private HyperLinkTMP text;

	protected override void Init()
	{
		text.AddHyperLinkClickListener(OnHyperLinkClick);
	}

	public override void ShowContent(AnnouncementOpenAPIModel data)
	{
		try
		{
			switch (data.AnnouncementMultiLangKind)
			{
			case AnnouncementMultiLangKind.Text:
				text.text = data.Payload;
				SetActiveAndDirty(active: true);
				break;
			case AnnouncementMultiLangKind.Html:
				text.text = JObject.Parse(data.Payload).Value<string>("meshPro");
				SetActiveAndDirty(active: true);
				break;
			default:
				Debug.LogError("Unsupported announcement rich text type.");
				break;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnHyperLinkClick(HyperLinkTMP.HyperlinkInfoTMP info)
	{
		AnnouncementWebViewOpener.Open(info.linkUrl);
	}
}
