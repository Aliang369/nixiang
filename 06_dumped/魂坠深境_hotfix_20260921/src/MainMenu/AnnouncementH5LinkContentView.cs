using ChillyRoom.Announcement.V1;
using ChillyRoom.Utils.TMPExtension;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MainMenu;

public class AnnouncementH5LinkContentView : AnnouncementContentView
{
	private readonly struct LinkPayload(string url, string title)
	{
		public readonly string Url = url;

		public readonly string Title = title;
	}

	[SerializeField]
	private HyperLinkTMP text;

	protected override void Init()
	{
		text.AddHyperLinkClickListener(OnHyperLinkClick);
	}

	public override void ShowContent(AnnouncementOpenAPIModel data)
	{
		LinkPayload linkPayload = ExtractLinkPayload(data.Payload);
		if (string.IsNullOrEmpty(linkPayload.Url))
		{
			Debug.LogError("[AnnouncementH5LinkContentView] Link payload has no url: " + data.Payload);
			this.text.text = "";
			SetActiveAndDirty(active: true);
			return;
		}
		string text = linkPayload.Url.Replace("\"", "&quot;");
		string text2 = linkPayload.Title.Replace("<", "&lt;").Replace(">", "&gt;");
		this.text.text = "<link=\"" + text + "\">" + text2 + "</link>";
		SetActiveAndDirty(active: true);
	}

	private void OnHyperLinkClick(HyperLinkTMP.HyperlinkInfoTMP info)
	{
		AnnouncementWebViewOpener.Open(info.linkUrl);
	}

	private static LinkPayload ExtractLinkPayload(string payload)
	{
		if (string.IsNullOrEmpty(payload))
		{
			return new LinkPayload(payload, payload);
		}
		try
		{
			return ExtractLinkPayload(JObject.Parse(payload));
		}
		catch
		{
			return new LinkPayload(payload, payload);
		}
	}

	private static LinkPayload ExtractLinkPayload(JObject payload)
	{
		JToken jToken = payload["AnnouncementDatas"]?.First ?? payload["announcementDatas"]?.First;
		string text = payload.Value<string>("url") ?? payload.Value<string>("h5Url") ?? payload.Value<string>("webUrl") ?? payload.Value<string>("webViewUrl") ?? payload.Value<string>("openUrl") ?? payload.Value<string>("OpenUrl") ?? jToken?["OpenUrl"]?.Value<string>() ?? jToken?["openUrl"]?.Value<string>();
		string title = jToken?["Title"]?.Value<string>() ?? jToken?["title"]?.Value<string>() ?? payload.Value<string>("title") ?? payload.Value<string>("Title") ?? text;
		return new LinkPayload(text, title);
	}
}
