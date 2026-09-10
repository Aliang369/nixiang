using System;
using System.Collections;

namespace CommonUtil.NShare;

public class NativeShareManager
{
	public static IEnumerator Share(Action<bool> onShareSuccess)
	{
		yield return Share(onShareSuccess, null, null);
	}

	public static IEnumerator Share(Action<bool> onShareSuccess, Action onBeforeCapture, Action onAfterCapture)
	{
		TraceLog.Debug("user trigger share ...");
		yield return NativeShareAdapter.TakeScreenshotAndShare(onShareSuccess, onBeforeCapture, onAfterCapture);
	}
}
