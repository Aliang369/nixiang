using System;
using UnityEngine;

public class UISafeArea : MonoBehaviour
{
	[Serializable]
	public class Config
	{
		public float minAspectRatio = 1.7777778f;

		public float maxAspectRatio = 2.3333333f;

		public DeviceConfig[] configs;
	}

	[Serializable]
	public class DeviceConfig
	{
		public string id;

		public float left;

		public float right;
	}

	private static bool refreshing;

	public SavingUtil.GlobalConfigSaveData globalConfigData => Bootstrap.instance?.globalSaveConfigData;

	private static event Action refreshAll;

	private void Awake()
	{
		refreshAll += RefreshInternal;
	}

	private void OnDestroy()
	{
		refreshAll -= RefreshInternal;
	}

	public void Refresh()
	{
		if (refreshing)
		{
			return;
		}
		refreshing = true;
		try
		{
			Canvas.ForceUpdateCanvases();
			refreshAll?.Invoke();
		}
		finally
		{
			refreshing = false;
		}
	}

	private void RefreshInternal()
	{
		if (globalConfigData != null)
		{
			RectTransform obj = base.transform as RectTransform;
			RectTransform trans = obj.parent as RectTransform;
			TryAutoInitSafeArea(new Vector2((float)Screen.width / trans.GetWidth(), (float)Screen.height / trans.GetHeight()).x);
			float left = globalConfigData.safeAreaLeft * 130f;
			float right = globalConfigData.safeAreaLeft * 130f;
			float bottom = 0f;
			float top = 0f;
			ApplyAspectRatioLimit(trans.GetWidth(), trans.GetHeight(), ref left, ref right, ref bottom, ref top);
			obj.offsetMin = new Vector2(left, bottom);
			obj.offsetMax = new Vector2(0f - right, 0f - top);
		}
	}

	private void TryAutoInitSafeArea(float factorX)
	{
		if (!globalConfigData.safeAreaAutoInited)
		{
			globalConfigData.safeAreaAutoInited = true;
			if (PlatformHelper.IsMobile() && factorX > 0f)
			{
				float xMin = Screen.safeArea.xMin;
				float b = (float)Screen.width - Screen.safeArea.xMax;
				float num = Mathf.Max(xMin, b);
				globalConfigData.safeAreaLeft = num / factorX / 130f;
			}
			globalConfigData.Save();
		}
	}

	private void ApplyAspectRatioLimit(float parentWidth, float parentHeight, ref float left, ref float right, ref float bottom, ref float top)
	{
		Config safeAreaConfig = GlobalConfigManager.instance.safeAreaConfig;
		if (safeAreaConfig.minAspectRatio > 0f && safeAreaConfig.maxAspectRatio > 0f && safeAreaConfig.minAspectRatio > safeAreaConfig.maxAspectRatio)
		{
			Debug.LogError($"[UISafeArea] Invalid aspect ratio limit: minAspectRatio({safeAreaConfig.minAspectRatio}) > maxAspectRatio({safeAreaConfig.maxAspectRatio})");
			return;
		}
		float num = parentWidth - left - right;
		float num2 = parentHeight - bottom - top;
		if (!(num <= 0f) && !(num2 <= 0f))
		{
			if (safeAreaConfig.maxAspectRatio > 0f && num / num2 > safeAreaConfig.maxAspectRatio)
			{
				float num3 = (num - num2 * safeAreaConfig.maxAspectRatio) * 0.5f;
				left += num3;
				right += num3;
				num = parentWidth - left - right;
			}
			if (safeAreaConfig.minAspectRatio > 0f && num / num2 < safeAreaConfig.minAspectRatio)
			{
				float num4 = (num2 - num / safeAreaConfig.minAspectRatio) * 0.5f;
				bottom += num4;
				top += num4;
			}
		}
	}
}
