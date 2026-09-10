using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CommonUtil.NShare;

public class NativeShareAdapter
{
	public static IEnumerator TakeScreenshotAndShare(Action<bool> onShareSuccess)
	{
		yield return TakeScreenshotAndShare(onShareSuccess, null, null);
	}

	public static IEnumerator TakeScreenshotAndShare(Action<bool> onShareSuccess, Action onBeforeCapture, Action onAfterCapture)
	{
		string term_share_subject = ScriptableObjectSingleton<TextDataPreset>.instance.term_share_subject;
		string term_share_text = ScriptableObjectSingleton<TextDataPreset>.instance.term_share_text;
		string url = GlobalConfigManager.instance.CurrentDistroConfigInfo?.ShareLink;
		yield return TakeScreenshotAndShare(term_share_subject, term_share_text, url, onShareSuccess, onBeforeCapture, onAfterCapture);
	}

	public static IEnumerator TakeScreenshotAndShare(string subject, string text, string url, Action<bool> onShareSuccess)
	{
		yield return TakeScreenshotAndShare(subject, text, url, onShareSuccess, null, null);
	}

	public static IEnumerator TakeScreenshotAndShare(string subject, string text, string url, Action<bool> onShareSuccess, Action onBeforeCapture, Action onAfterCapture)
	{
		Texture2D texture2D;
		try
		{
			onBeforeCapture?.Invoke();
			yield return new WaitForEndOfFrame();
			texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
			texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
			texture2D.Apply();
		}
		finally
		{
			onAfterCapture?.Invoke();
		}
		string text2 = Path.Combine(Application.temporaryCachePath, "shared img.png");
		File.WriteAllBytes(text2, ImageConversion.EncodeToPNG(texture2D));
		TraceLog.Debug("save screenshot to " + text2);
		UnityEngine.Object.Destroy(texture2D);
		NativeShare val = new NativeShare();
		val.AddFile(text2, (string)null);
		if (!string.IsNullOrEmpty(subject))
		{
			val.SetSubject(subject);
		}
		if (!string.IsNullOrEmpty(text))
		{
			val.SetText(text);
		}
		if (!string.IsNullOrEmpty(url))
		{
			val.SetUrl(url);
		}
		val.SetCallback((ShareResultCallback)delegate(ShareResult result, string shareTarget)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Invalid comparison between Unknown and I4
			TraceLog.Debug($" subject:{subject} text:{text} url:{url} \n Share result: {result} selected app: {shareTarget}");
			bool obj = (int)result == 1 || (int)result == 0;
			if (onShareSuccess != null)
			{
				onShareSuccess(obj);
			}
		});
		ShareAndroid(val);
	}

	private static void ShareAndroid(NativeShare share)
	{
		if (NativeShare.TargetExists("com.whatsapp", (string)null))
		{
			share.AddTarget("com.whatsapp", (string)null);
		}
		share.Share();
	}

	private static void ShareOther(NativeShare share)
	{
		share.Share();
	}
}
