using System;
using ChillyRoom.Announcement.V1;
using ChillyRoom.Webview;
using Common.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MainMenu;

public class AnnouncementH5PageContentView : AnnouncementContentView, IFrameUpdate
{
	private const string AnnouncementViewportScript = "(function () {\n    var meta = document.querySelector('meta[name=\"viewport\"]');\n    if (!meta) {\n        meta = document.createElement('meta');\n        meta.setAttribute('name', 'viewport');\n        document.head.appendChild(meta);\n    }\n    meta.setAttribute('content', 'width=device-width,height=device-height,initial-scale=1,maximum-scale=1,minimum-scale=1,user-scalable=no,viewport-fit=cover');\n\n    var style = document.getElementById('announcement-viewport-style');\n    if (!style) {\n        style = document.createElement('style');\n        style.id = 'announcement-viewport-style';\n        document.head.appendChild(style);\n    }\n    style.textContent = 'html{width:100vw!important;height:100vh!important;overflow:hidden!important;}body{width:100vw!important;min-height:100vh!important;margin:0!important;padding:0!important;box-sizing:border-box!important;overflow-x:hidden!important;overflow-y:auto!important;}body *{max-width:100%!important;box-sizing:border-box!important;}img,video,canvas{height:auto!important;}::-webkit-scrollbar{display:none!important;width:0!important;height:0!important;}';\n    document.documentElement.scrollLeft = 0;\n    if (document.body) {\n        document.body.scrollLeft = 0;\n    }\n})();";

	[SerializeField]
	private RectTransform webViewRectTransform;

	[NonSerialized]
	private WebViewClient webViewClient;

	[NonSerialized]
	private bool webViewShowing;

	protected override void Init()
	{
		webViewClient = new WebViewClient(new WebviewSetting
		{
			IsFullScreen = false,
			IsShowToolBar = false,
			IsUseWideViewPort = true,
			ReferenceRectTransform = webViewRectTransform
		});
		UniWebView orCreateUniWebView = webViewClient.GetOrCreateUniWebView();
		orCreateUniWebView.SetLoadWithOverviewMode(flag: false);
		orCreateUniWebView.SetHorizontalScrollBarEnabled(enabled: false);
		orCreateUniWebView.SetVerticalScrollBarEnabled(enabled: false);
		WebViewClient obj = webViewClient;
		obj.OnPageLoadError = (Action<WebViewClient, int, string>)Delegate.Remove(obj.OnPageLoadError, new Action<WebViewClient, int, string>(OnPageLoadError));
		WebViewClient obj2 = webViewClient;
		obj2.OnPageLoadError = (Action<WebViewClient, int, string>)Delegate.Combine(obj2.OnPageLoadError, new Action<WebViewClient, int, string>(OnPageLoadError));
		WebViewClient obj3 = webViewClient;
		obj3.OnPageLoadFinish = (Action<WebViewClient, int, string>)Delegate.Remove(obj3.OnPageLoadFinish, new Action<WebViewClient, int, string>(OnPageLoadFinish));
		WebViewClient obj4 = webViewClient;
		obj4.OnPageLoadFinish = (Action<WebViewClient, int, string>)Delegate.Combine(obj4.OnPageLoadFinish, new Action<WebViewClient, int, string>(OnPageLoadFinish));
	}

	protected override void Dispose()
	{
		WebViewClient obj = webViewClient;
		obj.OnPageLoadError = (Action<WebViewClient, int, string>)Delegate.Remove(obj.OnPageLoadError, new Action<WebViewClient, int, string>(OnPageLoadError));
		WebViewClient obj2 = webViewClient;
		obj2.OnPageLoadFinish = (Action<WebViewClient, int, string>)Delegate.Remove(obj2.OnPageLoadFinish, new Action<WebViewClient, int, string>(OnPageLoadFinish));
		webViewClient.Close();
	}

	public override void ShowContent(AnnouncementOpenAPIModel data)
	{
		Debug.Log($"[AnnouncementH5PageContentView] ShowContent({data.AnnouncementMultiLangKind}) Payload: {data.Payload}");
		SetActiveAndDirty(active: true);
		Canvas.ForceUpdateCanvases();
		Debug.Log($"[AnnouncementH5PageContentView] WebView screen rect: {GetScreenRect()}");
		string text = ExtractUrl(data.Payload);
		if (!text.IsNullOrEmpty())
		{
			Debug.Log("[AnnouncementH5PageContentView] Load url: " + text);
			webViewClient.ShowOrCreateWebview(text, new OverrideWebviewSetting
			{
				IsShowToolBar = false
			});
		}
		else
		{
			string text2 = ExtractHtml(data.Payload);
			Debug.Log($"[AnnouncementH5PageContentView] Load html length: {text2.Length}");
			webViewClient.LoadHTMLString(text2, "", skipBaseUrl: true);
			webViewClient.Show(new OverrideWebviewSetting
			{
				IsShowToolBar = false
			});
		}
		webViewClient.UpdateFrame();
		webViewShowing = true;
	}

	public override void HideContent()
	{
		if (webViewClient.GetUniWebView() != null)
		{
			webViewClient.Close();
		}
		webViewShowing = false;
		base.HideContent();
	}

	public void FrameUpdate()
	{
		if (webViewShowing && webViewClient.GetUniWebView() != null)
		{
			webViewClient.UpdateFrame();
		}
	}

	private static string ExtractUrl(string payload)
	{
		if (payload.IsNullOrEmpty())
		{
			return payload;
		}
		if (payload.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || payload.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return payload;
		}
		if (payload.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
		{
			return "https://" + payload;
		}
		try
		{
			JObject jObject = JObject.Parse(payload);
			string text = jObject.Value<string>("url") ?? jObject.Value<string>("h5Url") ?? jObject.Value<string>("webUrl") ?? jObject.Value<string>("webViewUrl") ?? jObject.Value<string>("openUrl") ?? jObject.Value<string>("OpenUrl");
			if (!text.IsNullOrEmpty() && text.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
			{
				return "https://" + text;
			}
			return text;
		}
		catch
		{
			return null;
		}
	}

	private static string ExtractHtml(string payload)
	{
		try
		{
			return JObject.Parse(payload).Value<string>("html") ?? payload;
		}
		catch
		{
			return payload;
		}
	}

	private void OnPageLoadError(WebViewClient client, int errorCode, string errorMessage)
	{
		Debug.LogError($"[AnnouncementH5PageContentView] Page load error. Code: {errorCode}, Message: {errorMessage}");
	}

	private void OnPageLoadFinish(WebViewClient client, int statusCode, string url)
	{
		Debug.Log($"[AnnouncementH5PageContentView] Page load finish. StatusCode: {statusCode}, Url: {url}");
		webViewClient.EvaluateJavaScript("(function () {\n    var meta = document.querySelector('meta[name=\"viewport\"]');\n    if (!meta) {\n        meta = document.createElement('meta');\n        meta.setAttribute('name', 'viewport');\n        document.head.appendChild(meta);\n    }\n    meta.setAttribute('content', 'width=device-width,height=device-height,initial-scale=1,maximum-scale=1,minimum-scale=1,user-scalable=no,viewport-fit=cover');\n\n    var style = document.getElementById('announcement-viewport-style');\n    if (!style) {\n        style = document.createElement('style');\n        style.id = 'announcement-viewport-style';\n        document.head.appendChild(style);\n    }\n    style.textContent = 'html{width:100vw!important;height:100vh!important;overflow:hidden!important;}body{width:100vw!important;min-height:100vh!important;margin:0!important;padding:0!important;box-sizing:border-box!important;overflow-x:hidden!important;overflow-y:auto!important;}body *{max-width:100%!important;box-sizing:border-box!important;}img,video,canvas{height:auto!important;}::-webkit-scrollbar{display:none!important;width:0!important;height:0!important;}';\n    document.documentElement.scrollLeft = 0;\n    if (document.body) {\n        document.body.scrollLeft = 0;\n    }\n})();", delegate(WebViewClient _, WebViewClient.JSEvaluateResult result)
		{
			if (!result.IsOk)
			{
				Debug.LogError("[AnnouncementH5PageContentView] Apply viewport script failed. Code: " + result.ResultCode + ", Message: " + result.Extra);
			}
		});
	}

	private Rect GetScreenRect()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[4];
		webViewRectTransform.GetWorldCorners(array);
		Canvas componentInParent = webViewRectTransform.GetComponentInParent<Canvas>();
		if ((UnityEngine.Object)(object)componentInParent != null && (int)componentInParent.renderMode != 0 && componentInParent.worldCamera != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = componentInParent.worldCamera.WorldToScreenPoint(array[i]);
			}
		}
		Vector3 vector = array[1];
		Vector3 vector2 = array[3];
		return new Rect(vector.x, (float)Screen.height - vector.y, vector2.x - vector.x, vector.y - vector2.y);
	}
}
