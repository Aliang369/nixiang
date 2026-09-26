using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbUtil
{
	private class ElementOriginalColor
	{
		public Color? textColor;

		public Color? backgroundTintColor;
	}

	private static Dictionary<string, SkeletonDataAsset> _sharedSkeletonAssetCache;

	private static Dictionary<string, Sprite> _sharedSpriteCache;

	private static HashSet<VisualElement> s_fixedBgElements;

	private static readonly FieldInfo s_BackgroundSpriteField;

	public static string Language(string key)
	{
		return zbLanguage.Instance.GetStringTable(key);
	}

	public static string GetContentString(ContentDetail content)
	{
		string format = Language(content.Label);
		List<object> list = new List<object>();
		foreach (string item in content.Params)
		{
			string text = Language(item);
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
			else
			{
				list.Add(item);
			}
		}
		return string.Format(format, list.ToArray());
	}

	public static string FormatTimeYYYYMMDD(long timeInSeconds)
	{
		return DateTimeOffset.FromUnixTimeSeconds(timeInSeconds).DateTime.ToString("yyyy-MM-dd");
	}

	public static string FormatUnixTimeToUtcString(long unixTime)
	{
		return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().ToString("M-d HH:mm:ss");
	}

	public static string ReplaceUtcTimeToLocal(string input)
	{
		string pattern = "(\\d{4}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2})\\s*\\(UTC\\s*([+-]?\\s*\\d+)\\)";
		return Regex.Replace(input, pattern, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			int num = int.Parse(match.Groups[2].Value.Replace(" ", ""));
			DateTime dateTime = DateTime.ParseExact(value, "yyyy/MM/dd HH:mm:ss", null);
			return new DateTimeOffset(dateTime, TimeSpan.FromHours((double)num)).ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
		});
	}

	public static string FormatUnixTimeToHHMM(long timestamp)
	{
		return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("HH:mm");
	}

	public static string FormatUnixTimeToSimpleString(long unixTime)
	{
		return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToOffset(TimeSpan.FromHours(9.0)).DateTime.ToString("M-d HH:mm:ss");
	}

	public static long GetTimestampOfDayInCurrentMonthByServerTime(long serverTimestampSeconds, int dayOfMonth)
	{
		if (dayOfMonth <= 0)
		{
			dayOfMonth = 1;
		}
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(serverTimestampSeconds);
		int num = DateTime.DaysInMonth(dateTimeOffset.Year, dateTimeOffset.Month);
		if (dayOfMonth > num)
		{
			dayOfMonth = num;
		}
		return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dayOfMonth, 0, 0, 0, dateTimeOffset.Offset).ToUnixTimeSeconds();
	}

	public static string FormatTimeHHMMSS(long seconds, bool displayDay = true)
	{
		if (seconds < 0)
		{
			seconds = 0L;
		}
		long num = seconds;
		long num2 = num / 3600;
		long num3 = num % 3600 / 60;
		long num4 = num % 60;
		if ((num2 >= 24) & displayDay)
		{
			return string.Format("{0}天 {1:D2}:{2:D2}:{3:D2}", new object[4]
			{
				num2 / 24,
				num2 % 24,
				num3,
				num4
			});
		}
		return $"{num2 % 24:D2}:{num3:D2}:{num4:D2}";
	}

	public static string FormatTimeMMSS(long timeInSeconds)
	{
		if (timeInSeconds < 0)
		{
			timeInSeconds = 0L;
		}
		int num = Mathf.FloorToInt((float)(timeInSeconds % 3600 / 60));
		int num2 = Mathf.FloorToInt((float)(timeInSeconds % 60));
		return $"{num:D2}:{num2:D2}";
	}

	public static string FormatTimeSmart(long timeInSeconds)
	{
		if (timeInSeconds < 0)
		{
			timeInSeconds = 0L;
		}
		long num = timeInSeconds;
		long num2 = num / 86400;
		long num3 = num % 86400 / 3600;
		long num4 = num % 3600 / 60;
		long num5 = num % 60;
		if (num2 >= 1)
		{
			return $"{num2} 天";
		}
		if (num3 >= 1)
		{
			return $"{num3} 小时";
		}
		return $"{num4:D2}:{num5:D2}";
	}

	public static string FormatTimeSmartFormatTimeToHMS(long timeInSeconds)
	{
		if (timeInSeconds < 0)
		{
			timeInSeconds = 0L;
		}
		long num = timeInSeconds / 3600;
		long num2 = timeInSeconds % 3600 / 60;
		long num3 = timeInSeconds % 60;
		return $"{num}:{num2:D2}:{num3:D2}";
	}

	public static bool IsSameDay(long t1, long t2)
	{
		return t1 / 86400 == t2 / 86400;
	}

	public static string ConvertToTBMK(long num, bool isFloor = true)
	{
		if (num >= 100000000)
		{
			return FormatNumber(num, 100000000L, "亿", isFloor);
		}
		if (num >= 10000)
		{
			return FormatNumber(num, 10000L, "万", isFloor);
		}
		return num.ToString();
	}

	private static string FormatNumber(long num, long divisor, string suffix, bool isFloor)
	{
		double num2 = (double)num / (double)divisor;
		return ((!isFloor) ? (Math.Ceiling(num2 * 10.0) / 10.0) : (Math.Floor(num2 * 10.0) / 10.0)).ToString("0.#") + suffix;
	}

	public static bool IsCurrentMonth(long timestamp)
	{
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
		DateTimeOffset dateTimeOffset2 = DateTimeOffset.FromUnixTimeSeconds(zbSelf.Instance.timeHandler.GetServerTime());
		if (dateTimeOffset.Year == dateTimeOffset2.Year)
		{
			return dateTimeOffset.Month == dateTimeOffset2.Month;
		}
		return false;
	}

	public static Vector2 ScreenToUIToolkit(Vector2 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.height;
		float x = screenPosition.x;
		float num2 = num - screenPosition.y;
		return new Vector2(x, num2);
	}

	public static Vector2 UIToolkitToScreen(Vector2 uiPosition, VisualElement element)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		if (element == null)
		{
			return Vector2.zero;
		}
		float num = Screen.height;
		float uIScale = zbUIScaler.GetUIScale();
		float num2 = uiPosition.x * uIScale;
		float num3 = num - uiPosition.y * uIScale;
		return new Vector2(num2, num3);
	}

	public static Vector3 UIToolkitToWorldSpace(Vector2 uiPosition, Camera camera, VisualElement element)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ScreenToWorldSpace(UIToolkitToScreen(uiPosition, element), camera);
	}

	public static Vector3 ScreenToWorldSpace(Vector2 screenPosition, Camera camera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
	}

	public static void RemoveChildren(GameObject obj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in obj.transform)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	public static float GetV2SqrDistance2D(Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = a - b;
		return ((Vector2)(ref val)).sqrMagnitude;
	}

	public static float GetV3SqrDistance2D(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return num * num + num2 * num2;
	}

	public static float GetSplitV3SqrDistance2D(float x1, float y1, float x2, float y2)
	{
		float num = x1 - x2;
		float num2 = y1 - y2;
		return num * num + num2 * num2;
	}

	public static float GetVector2Distance(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public static Vector3 GetTargetNormalized(Vector3 startPosition, Vector3 targetPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(targetPosition - startPosition);
		return Vector2.op_Implicit(((Vector2)(ref val)).normalized);
	}

	public static GameObject FindGameObjectNoRecursive(Transform parent, string strObjectName)
	{
		return ((Component)((Component)parent).transform.Find(strObjectName)).gameObject;
	}

	public static GameObject FindGameObjectRecursive(Transform parent, string name)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (((Object)parent).name == name)
		{
			return ((Component)parent).gameObject;
		}
		foreach (Transform item in parent)
		{
			GameObject val = FindGameObjectRecursive(item, name);
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
		}
		return null;
	}

	static zbUtil()
	{
		_sharedSkeletonAssetCache = new Dictionary<string, SkeletonDataAsset>();
		_sharedSpriteCache = new Dictionary<string, Sprite>();
		s_fixedBgElements = new HashSet<VisualElement>();
		s_BackgroundSpriteField = typeof(Background).GetField("m_Sprite", BindingFlags.Instance | BindingFlags.NonPublic);
		SpineVisualElement.AssetUrlResolver = ResolveSpineAssetFromUrl;
		zbPieProgressTex.SpriteUrlResolver = ResolveSpriteFromUrl;
	}

	private static Sprite ExtractSprite(Background bg)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (s_BackgroundSpriteField == null)
		{
			return null;
		}
		object? value = s_BackgroundSpriteField.GetValue(bg);
		return (Sprite)((value is Sprite) ? value : null);
	}

	public static void FixBackground(VisualElement element)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (element == null)
		{
			return;
		}
		StyleBackground backgroundImage = element.style.backgroundImage;
		Background value = ((StyleBackground)(ref backgroundImage)).value;
		if ((Object)(object)((Background)(ref value)).texture != (Object)null)
		{
			s_fixedBgElements.Add(element);
			return;
		}
		Sprite val = ExtractSprite(value) ?? ExtractSprite(element.resolvedStyle.backgroundImage);
		if (!((Object)(object)val == (Object)null) && !((Object)(object)val.texture == (Object)null))
		{
			Vector4 border = val.border;
			if (border.x > 0f)
			{
				element.style.unitySliceLeft = StyleInt.op_Implicit((int)border.x);
			}
			if (border.z > 0f)
			{
				element.style.unitySliceRight = StyleInt.op_Implicit((int)border.z);
			}
			if (border.w > 0f)
			{
				element.style.unitySliceTop = StyleInt.op_Implicit((int)border.w);
			}
			if (border.y > 0f)
			{
				element.style.unitySliceBottom = StyleInt.op_Implicit((int)border.y);
			}
			element.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val.texture));
			s_fixedBgElements.Add(element);
		}
	}

	public static void FixAllBackgrounds(VisualElement element)
	{
		if (element == null || s_fixedBgElements.Contains(element))
		{
			return;
		}
		FixBackground(element);
		foreach (VisualElement item in element.Children())
		{
			FixAllBackgrounds(item);
		}
	}

	private static Sprite ResolveSpriteFromUrl(string projectUrl)
	{
		if (string.IsNullOrEmpty(projectUrl))
		{
			return null;
		}
		int num = projectUrl.IndexOf("database/");
		if (num < 0)
		{
			return null;
		}
		string text = projectUrl.Substring(num + 9);
		int num2 = text.IndexOf('?');
		if (num2 >= 0)
		{
			text = text.Substring(0, num2);
		}
		if (text.StartsWith("Assets/"))
		{
			text = text.Substring(7);
		}
		if (!_sharedSpriteCache.TryGetValue(text, out var value))
		{
			zbAssetLoader instance = zbAssetLoader.Instance;
			if (instance == null)
			{
				return null;
			}
			value = instance.LoadResouce<Sprite>(text);
			if (!((Object)(object)value != (Object)null))
			{
				return null;
			}
			_sharedSpriteCache[text] = value;
		}
		return value;
	}

	private static SkeletonDataAsset ResolveSpineAssetFromUrl(string projectUrl)
	{
		if (string.IsNullOrEmpty(projectUrl))
		{
			return null;
		}
		int num = projectUrl.IndexOf("database/");
		if (num < 0)
		{
			return null;
		}
		string text = projectUrl.Substring(num + 9);
		int num2 = text.IndexOf('?');
		if (num2 >= 0)
		{
			text = text.Substring(0, num2);
		}
		if (text.StartsWith("Assets/"))
		{
			text = text.Substring(7);
		}
		if (!_sharedSkeletonAssetCache.TryGetValue(text, out var value))
		{
			zbAssetLoader instance = zbAssetLoader.Instance;
			if (instance == null)
			{
				return null;
			}
			value = instance.LoadResouce<SkeletonDataAsset>(text);
			if (!((Object)(object)value != (Object)null))
			{
				return null;
			}
			_sharedSkeletonAssetCache[text] = value;
		}
		return value;
	}

	public static void RefreshSpine(SkeletonAnimation skeletonAnimation, string spinePath, string animaiton = "", bool loop = false)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			if (skeletonAnimation.userCacheAssetName != spinePath)
			{
				skeletonAnimation.userCacheAssetName = spinePath;
				if (!_sharedSkeletonAssetCache.TryGetValue(spinePath, out var value))
				{
					value = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
					if (!((Object)(object)value != (Object)null))
					{
						return;
					}
					_sharedSkeletonAssetCache[spinePath] = value;
				}
				if ((Object)(object)((SkeletonRenderer)skeletonAnimation).skeletonDataAsset != (Object)(object)value || !((SkeletonRenderer)skeletonAnimation).valid)
				{
					MeshFilter component = ((Component)skeletonAnimation).GetComponent<MeshFilter>();
					if ((Object)(object)component != (Object)null)
					{
						component.sharedMesh = null;
					}
					((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = value;
					((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
				}
				if ((Object)(object)skeletonAnimation != (Object)null && animaiton.Length > 0)
				{
					try
					{
						skeletonAnimation.AnimationState.SetAnimation(0, animaiton, loop);
					}
					catch (Exception)
					{
					}
				}
			}
			else if (animaiton.Length > 0 && ((SkeletonRenderer)skeletonAnimation).valid && skeletonAnimation.AnimationState != null)
			{
				TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(0);
				if (current == null || current.Animation.Name != animaiton)
				{
					try
					{
						skeletonAnimation.AnimationState.SetAnimation(0, animaiton, loop);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			MeshRenderer component2 = ((Component)skeletonAnimation).GetComponent<MeshRenderer>();
			if ((Object)(object)component2 != (Object)null && !((Renderer)component2).enabled && (Object)(object)((Component)skeletonAnimation).GetComponent<SkeletonRenderSeparator>() == (Object)null)
			{
				((Renderer)component2).enabled = true;
			}
		}
	}

	public static void ClearSpineCache()
	{
		_sharedSkeletonAssetCache.Clear();
		_sharedSpriteCache.Clear();
	}

	public static void SetLabelIfChanged(Label label, string text)
	{
		if (label != null)
		{
			string text2 = text ?? string.Empty;
			if (((TextElement)label).text != text2)
			{
				((TextElement)label).text = text2;
			}
		}
	}

	public static void SetSpineVisibleWithFreeze(SpineVisualElement spine, bool visible)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (spine != null)
		{
			DisplayStyle val = (DisplayStyle)(!visible);
			if (((VisualElement)spine).style.display != StyleEnum<DisplayStyle>.op_Implicit(val))
			{
				((VisualElement)spine).style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
			}
			spine.freeze = !visible;
		}
	}

	public static void PreheatSkeletonData(string spinePath)
	{
		if (!string.IsNullOrEmpty(spinePath) && !_sharedSkeletonAssetCache.ContainsKey(spinePath))
		{
			SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
			if (!((Object)(object)val == (Object)null))
			{
				_sharedSkeletonAssetCache[spinePath] = val;
				val.GetSkeletonData(true);
			}
		}
	}

	public static void PreheatPrefabSkeletonData(string prefabPath)
	{
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>(prefabPath);
		if (!((Object)(object)val == (Object)null))
		{
			SkeletonRenderer componentInChildren = val.GetComponentInChildren<SkeletonRenderer>(true);
			if ((Object)(object)componentInChildren != (Object)null && (Object)(object)componentInChildren.skeletonDataAsset != (Object)null)
			{
				componentInChildren.skeletonDataAsset.GetSkeletonData(true);
			}
		}
	}

	public static void RefreshSpineUI(SpineVisualElement skeletonAnimation, string spinePath, string animaiton = "", bool loop = false)
	{
		if (skeletonAnimation == null)
		{
			return;
		}
		if (skeletonAnimation.userCacheAssetName != spinePath)
		{
			skeletonAnimation.userCacheAssetName = spinePath;
			if (!_sharedSkeletonAssetCache.TryGetValue(spinePath, out var value))
			{
				value = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
				if (!((Object)(object)value != (Object)null))
				{
					return;
				}
				_sharedSkeletonAssetCache[spinePath] = value;
			}
			if ((Object)(object)skeletonAnimation.skeletonDataAsset != (Object)(object)value || !skeletonAnimation.IsValid)
			{
				skeletonAnimation.skeletonDataAsset = value;
				skeletonAnimation.Initialize(true);
			}
		}
		if (animaiton.Length <= 0)
		{
			return;
		}
		try
		{
			TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(0);
			if (current == null || current.Animation == null || current.Animation.Name != animaiton)
			{
				skeletonAnimation.AnimationState.SetAnimation(0, animaiton, loop);
			}
		}
		catch (Exception)
		{
		}
	}

	public static void RemoveClassNames(VisualElement ele, params string[] classNames)
	{
		foreach (string text in classNames)
		{
			ele.RemoveFromClassList(text);
		}
	}

	public static void SetTextFieldStyle(TextField textField, string imgPath, int left = 0, int right = 0, int top = 0, int bottom = 0)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(imgPath);
		if ((Object)(object)val != (Object)null)
		{
			VisualElement val2 = UQueryExtensions.Q((VisualElement)(object)textField, "unity-text-input", (string)null);
			val2.style.backgroundColor = new StyleColor(Color.clear);
			val2.style.backgroundImage = new StyleBackground(val);
			val2.style.unitySliceBottom = StyleInt.op_Implicit(bottom);
			val2.style.unitySliceTop = StyleInt.op_Implicit(top);
			val2.style.unitySliceLeft = StyleInt.op_Implicit(left);
			val2.style.unitySliceRight = StyleInt.op_Implicit(right);
			RemoveClassNames(val2, "unity-text-field", "unity-base-field", "unity-base-field--no-label", "unity-base-text-field");
			RemoveClassNames(UQueryExtensions.Q<VisualElement>(val2, "unity-text-input", (string)null), "unity-base-text-field__input");
		}
	}

	public static void SetSliderDraggerStyle(string imgPath, params Slider[] sliders)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(imgPath);
		for (int i = 0; i < sliders.Length; i++)
		{
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)sliders[i], "unity-dragger", (string)null).style.backgroundImage = new StyleBackground(val);
		}
	}

	public static void Debug2LogFile(string fileName, object ack)
	{
	}

	public static int GetActiveTouchCounts()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		int num = 0;
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if ((int)((Touch)(ref touch)).phase != 3 && (int)((Touch)(ref touch)).phase != 4)
			{
				num++;
			}
		}
		return num;
	}

	public static Vector3 GetZoomCenterDelta()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = Vector3.zero;
		List<Touch> list = new List<Touch>();
		if (Input.touchCount > 0)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if ((int)((Touch)(ref touch)).phase != 3 && (int)((Touch)(ref touch)).phase != 4)
				{
					list.Add(touch);
				}
			}
			if (list.Count >= 2)
			{
				Touch val = list[0];
				Vector2 position = ((Touch)(ref val)).position;
				val = list[1];
				Vector2 position2 = ((Touch)(ref val)).position;
				Vector2 val2 = (position + position2) / 2f;
				float num = Mathf.Abs(((Component)Camera.main).transform.position.z);
				result = Camera.main.ScreenToWorldPoint(new Vector3(val2.x, val2.y, num));
			}
		}
		else
		{
			Vector2 val3 = Vector2.op_Implicit(Input.mousePosition);
			float num2 = Mathf.Abs(((Component)Camera.main).transform.position.z);
			result = Camera.main.ScreenToWorldPoint(new Vector3(val3.x, val3.y, num2));
		}
		return result;
	}

	public static Vector2 GetWorldPosByToolKitPosition(IEventHandler eventHandler, Vector2 localPosition, Camera cam)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = (VisualElement)(object)((eventHandler is VisualElement) ? eventHandler : null);
		if (val == null)
		{
			return Vector2.zero;
		}
		Vector2 val2 = VisualElementExtensions.LocalToWorld(val, localPosition);
		float uIScale = zbUIScaler.GetUIScale();
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(val2.x * uIScale, (float)Screen.height - val2.y * uIScale);
		return Vector2.op_Implicit(cam.ScreenToWorldPoint(Vector2.op_Implicit(new Vector2(val3.x, val3.y))));
	}

	public static void CenterNodeItems(VisualElement[] nodes, float itemWidth = 120f, float gap = 10f)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (gap != 0f)
		{
			itemWidth += gap;
		}
		int num = 0;
		VisualElement[] array = nodes;
		for (int i = 0; i < array.Length; i++)
		{
			if ((int)array[i].resolvedStyle.display != 1)
			{
				num++;
			}
		}
		float num2 = (0f - (itemWidth * (float)num - gap)) / 2f + itemWidth / 2f - gap / 2f;
		int num3 = 0;
		array = nodes;
		foreach (VisualElement val in array)
		{
			if ((int)val.resolvedStyle.display != 1)
			{
				float num4 = num2 + (float)num3 * itemWidth;
				val.transform.position = new Vector3(num4, val.transform.position.y, val.transform.position.z);
				num3++;
			}
		}
	}

	public static VisualElement CreateHeroStarTag(VisualElement btn, Vector2? pos = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/hero/hero_star.png"));
		val.style.width = StyleLength.op_Implicit(40f);
		val.style.height = StyleLength.op_Implicit(40f);
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (pos.HasValue)
		{
			val.style.right = StyleLength.op_Implicit(pos.Value.x);
			val.style.top = StyleLength.op_Implicit(pos.Value.y);
		}
		else
		{
			val.style.right = StyleLength.op_Implicit(-10f);
			val.style.top = StyleLength.op_Implicit(-15f);
		}
		btn.Add(val);
		zbUIEffect.CreateUpDownAction(val, 5f, 1.5f);
		return val;
	}

	public static VisualElement CreateNewTag(VisualElement btn, Vector2? pos = null, float scale = 1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/common/new_blue.png"));
		val.style.width = StyleLength.op_Implicit(60f * scale);
		val.style.height = StyleLength.op_Implicit(60f * scale);
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (pos.HasValue)
		{
			val.style.right = StyleLength.op_Implicit(pos.Value.x);
			val.style.top = StyleLength.op_Implicit(pos.Value.y);
		}
		else
		{
			val.style.right = StyleLength.op_Implicit(-10f);
			val.style.top = StyleLength.op_Implicit(-15f);
		}
		btn.Add(val);
		zbUIEffect.CreateScaleAction(val, scale, scale * 1.15f, 1.5f);
		return val;
	}

	public static VisualElement CreateNotifyTag(VisualElement btn, Vector2? pos = null, float scale = 1f, bool scaleAnim = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/common/notifytag.png"));
		val.style.width = StyleLength.op_Implicit(30f * scale);
		val.style.height = StyleLength.op_Implicit(30f * scale);
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (pos.HasValue)
		{
			val.style.right = StyleLength.op_Implicit(pos.Value.x);
			val.style.top = StyleLength.op_Implicit(pos.Value.y);
		}
		else
		{
			val.style.right = StyleLength.op_Implicit(-3f);
			val.style.top = StyleLength.op_Implicit(-3f);
		}
		btn.Add(val);
		if (scaleAnim)
		{
			zbUIEffect.CreateScaleAction(val, scale, scale * 1.15f, 1.5f);
		}
		return val;
	}

	public static void CreateRowMoveAction(VisualElement element, float distance = 10f)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float initialY = element.resolvedStyle.translate.y;
		element.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>
		{
			new StylePropertyName("translate")
		});
		element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(1f, (TimeUnit)0)
		});
		element.style.transitionTimingFunction = StyleList<EasingFunction>.op_Implicit(new List<EasingFunction>
		{
			new EasingFunction((EasingMode)3)
		});
		bool movingRight = true;
		element.schedule.Execute((Action)delegate
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (movingRight)
			{
				element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(distance), Length.op_Implicit(initialY), 0f));
			}
			else
			{
				element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f - distance), Length.op_Implicit(initialY), 0f));
			}
			movingRight = !movingRight;
		}).Every(1000L);
	}

	public static void CreateRowUpDownAction(VisualElement element, float distance = 10f)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		float initialX = element.resolvedStyle.translate.x;
		float initialY = element.resolvedStyle.translate.y;
		element.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>
		{
			new StylePropertyName("translate")
		});
		element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(1f, (TimeUnit)0)
		});
		element.style.transitionTimingFunction = StyleList<EasingFunction>.op_Implicit(new List<EasingFunction>
		{
			new EasingFunction((EasingMode)3)
		});
		bool movingUp = true;
		element.schedule.Execute((Action)delegate
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (movingUp)
			{
				element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(initialX), Length.op_Implicit(initialY + distance), 0f));
			}
			else
			{
				element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(initialX), Length.op_Implicit(initialY - distance), 0f));
			}
			movingUp = !movingUp;
		}).Every(1000L);
	}

	public static void SetElementGray(VisualElement element, bool bGray)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (bGray)
		{
			if (!(element.userData is ElementOriginalColor))
			{
				ElementOriginalColor userData = new ElementOriginalColor
				{
					textColor = element.resolvedStyle.color,
					backgroundTintColor = element.resolvedStyle.unityBackgroundImageTintColor
				};
				element.userData = userData;
			}
			element.style.color = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
			element.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
		}
		else if (element.userData is ElementOriginalColor elementOriginalColor)
		{
			if (elementOriginalColor.textColor.HasValue)
			{
				element.style.color = StyleColor.op_Implicit(elementOriginalColor.textColor.Value);
			}
			if (elementOriginalColor.backgroundTintColor.HasValue)
			{
				element.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(elementOriginalColor.backgroundTintColor.Value);
			}
		}
		foreach (VisualElement item in element.Children())
		{
			SetElementGray(item, bGray);
		}
	}

	public static bool Vector3Equal(Vector3 a, Vector3 b, float epsilon = 0.0001f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon)
		{
			return Mathf.Abs(a.z - b.z) < epsilon;
		}
		return false;
	}

	public static bool Vector2Equal(Vector2 a, Vector2 b, float epsilon = 0.0001f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(a.x - b.x) < epsilon)
		{
			return Mathf.Abs(a.y - b.y) < epsilon;
		}
		return false;
	}

	public static bool IsPointInBox(Vector3 pos, Vector3 boxPos, float boxWidth, float boxHeight)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = boxWidth / 2f;
		float num2 = boxHeight / 2f;
		float num3 = pos.x - boxPos.x;
		if (num3 < 0f - num || num3 > num)
		{
			return false;
		}
		float num4 = pos.y - boxPos.y;
		if (num4 < 0f - num2 || num4 > num2)
		{
			return false;
		}
		return true;
	}

	public static bool Reached(Vector2 a, Vector2 b, float sqrEpsilon = 0.0001f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = a - b;
		return ((Vector2)(ref val)).sqrMagnitude <= sqrEpsilon;
	}

	public static bool Roll0to100(int percent)
	{
		return Rand.Range(0, 100) < percent;
	}

	public static bool Roll0to1(float chance)
	{
		return Rand.value < chance;
	}

	public static List<int> ParseIntArray(string str, char separator = ',')
	{
		if (string.IsNullOrEmpty(str))
		{
			return new List<int>();
		}
		return (from s in str.Split(separator)
			where !string.IsNullOrWhiteSpace(s)
			select s).Select(delegate(string s)
		{
			int.TryParse(s, out var result);
			return result;
		}).ToList();
	}

	public static Dictionary<string, string> ParseKeyValue(string str)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(str))
		{
			return dictionary;
		}
		string[] array = str.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrEmpty(text) && text.Contains("="))
			{
				string[] array2 = text.Split(new char[1] { '=' }, 2);
				string key = array2[0].Trim();
				string value = ((array2.Length > 1) ? array2[1].Trim() : "");
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, value);
				}
			}
		}
		return dictionary;
	}

	public static void ShowVoucherCostPop(float price, Action call = null)
	{
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(3, (long)price))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 3L, (long)price);
			return;
		}
		int consumption = zbSetSave.Instance.GetConsumption();
		if (price > 0f && consumption == 1)
		{
			zbUIMgr.Instance.ShowVoucherCostPop(price, call);
		}
		else
		{
			call?.Invoke();
		}
	}

	public static Texture2D GetTexture2DByName(int workerId)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		string wokerCaptureFileName = GetWokerCaptureFileName(workerId);
		if (!File.Exists(wokerCaptureFileName))
		{
			return null;
		}
		byte[] array = File.ReadAllBytes(wokerCaptureFileName);
		Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
		if (!ImageConversion.LoadImage(val, array))
		{
			return null;
		}
		return val;
	}

	public static string GetWokerCaptureFileName(int workerId)
	{
		return Path.Combine(Application.persistentDataPath, "spinecaptures/" + zbSelf.Instance.GetRoleId(), workerId + ".zbg");
	}

	public static string GetWokerCaptureFileNameStr(int workerId)
	{
		return Path.Combine(Application.persistentDataPath, "spinecaptures/" + zbSelf.Instance.GetRoleId(), workerId + ".str");
	}

	public static bool CheckAnimationNameExist(SkeletonAnimation skeletonAnimation, string animName)
	{
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			return false;
		}
		if (((SkeletonRenderer)skeletonAnimation).skeleton == null || ((SkeletonRenderer)skeletonAnimation).skeleton.Data == null)
		{
			return false;
		}
		return ((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animName) != null;
	}

	public static string LoadFromFile(string strFilePath)
	{
		try
		{
			if (!File.Exists(strFilePath))
			{
				return string.Empty;
			}
			byte[] bytes = zbFileEncrypUtil.DecryptFile(strFilePath);
			return Encoding.UTF8.GetString(bytes);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static string SetServerString(int serverId)
	{
		return "S" + serverId % 10000;
	}

	public static void CopyString(string str)
	{
		GUIUtility.systemCopyBuffer = str;
		zbUIMgr.Instance.BubbleInfo(Language("lang.10375"));
	}

	public static void MoveToLastSibling(GameObject obj)
	{
		if ((Object)(object)obj.transform.parent != (Object)null)
		{
			int siblingIndex = obj.transform.parent.childCount - 1;
			obj.transform.SetSiblingIndex(siblingIndex);
		}
	}

	public static string GetNeedHaveRichString(long haveCount, long needCount)
	{
		string text = ConvertToTBMK(haveCount);
		string text2 = ConvertToTBMK(needCount, isFloor: false);
		if (haveCount >= needCount)
		{
			return "<color=#2DF421>" + text + "</color>/" + text2;
		}
		return "<color=#C50000>" + text + "</color>/" + text2;
	}

	public static string AddHighlightStyle(string text, string color, int fontSize)
	{
		return $"<color=#{color}><size={fontSize}>{text}</size></color>";
	}

	public static void LinkToURLWithParams(string url)
	{
		string url2 = url;
		if (url.Contains("mydreamjoy.com"))
		{
			string deviceId = zbLoginUtil.deviceId;
			string stringToEscape = zbLoginUtil.guid.ToString();
			int appChannel = (int)zbLoginUtil.appChannel;
			string stringToEscape2 = appChannel.ToString();
			string resVersion = zbSdkHelper.GetResVersion();
			string stringToEscape3 = ((int)zbLanguage.Instance.GetLanguageType()).ToString();
			url2 = url + "/?imei=" + Uri.EscapeDataString(deviceId) + "&guid=" + Uri.EscapeDataString(stringToEscape) + "&channel=" + Uri.EscapeDataString(stringToEscape2) + "&version=" + Uri.EscapeDataString(resVersion) + "&lang=" + Uri.EscapeDataString(stringToEscape3);
		}
		zbSdkHelper.OpenURL(url2);
	}

	public static void LinkToURLWithoutParams(string url)
	{
		zbSdkHelper.OpenURL(url);
	}

	public static bool IsSameWorkerStyle(WorkerStyleInfo styleA, WorkerStyleInfo styleB)
	{
		if (styleA != null && styleB != null)
		{
			return styleA.ToString() == styleB.ToString();
		}
		return false;
	}

	public static void SetPickingModeIgnoreRecursive(VisualElement element)
	{
		element.pickingMode = (PickingMode)1;
		foreach (VisualElement item in element.Children())
		{
			SetPickingModeIgnoreRecursive(item);
		}
	}

	public static bool IsWorldBoundValid(VisualElement ve)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (ve != null && ve.panel != null)
		{
			Rect worldBound = ve.worldBound;
			return ((Rect)(ref worldBound)).size != Vector2.zero;
		}
		return false;
	}

	public static Vector3 GetWorldPositionFromVisualElement(VisualElement element, Camera uiCamera)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = UIToScreenPosition(element, zbUIMgr.Instance.GetPanelSettings());
		return uiCamera.ScreenToWorldPoint(new Vector3(val.x, val.y, uiCamera.nearClipPlane));
	}

	public static Vector2 UIToScreenPosition(VisualElement element, PanelSettings panelSettings)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		_ = element.panel;
		Rect worldBound = element.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		Vector2 val = Vector2Int.op_Implicit(panelSettings.referenceResolution);
		float num = Screen.width;
		float num2 = Screen.height;
		float match = panelSettings.match;
		float num3 = Mathf.Log(num / val.x, 2f);
		float num4 = Mathf.Log(num2 / val.y, 2f);
		float num5 = Mathf.Lerp(num3, num4, match);
		float num6 = Mathf.Pow(2f, num5);
		Vector2 val2 = position * num6;
		return new Vector2(val2.x, num2 - val2.y);
	}

	public static void Shuffle<T>(List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Rand.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void ScrollHorizontalToIndex(ScrollView scrollView, int canShowNum, int maxNum, float itemWidth, int index)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (index >= 0 && index <= maxNum)
		{
			if (index <= canShowNum / 2)
			{
				scrollView.scrollOffset = new Vector2(0f, scrollView.scrollOffset.y);
			}
			else
			{
				scrollView.scrollOffset = new Vector2((float)(index - canShowNum / 2) * itemWidth, scrollView.scrollOffset.y);
			}
		}
	}

	public static void ScrollVerticalToIndex(ScrollView scrollView, int canShowNum, int maxNum, float itemWidth, int index)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (index >= 0 && index <= maxNum)
		{
			if (index <= canShowNum / 2)
			{
				scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, 0f);
			}
			else
			{
				scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, (float)(index - canShowNum) * itemWidth);
			}
		}
	}

	public static void SetDisplayIfChanged(VisualElement element, DisplayStyle newValue)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (element.style.display != StyleEnum<DisplayStyle>.op_Implicit(newValue))
		{
			element.style.display = StyleEnum<DisplayStyle>.op_Implicit(newValue);
		}
	}

	public static void SetWidthIfChanged(VisualElement element, float newWidthPx)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		StyleLength width = element.style.width;
		if ((int)((StyleLength)(ref width)).keyword == 0)
		{
			Length value = ((StyleLength)(ref width)).value;
			if (Mathf.Approximately(((Length)(ref value)).value, newWidthPx))
			{
				return;
			}
		}
		element.style.width = StyleLength.op_Implicit(new Length(newWidthPx, (LengthUnit)0));
	}

	public static void SetWidthIfChanged(VisualElement element, Length newWidth)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		StyleLength width = element.style.width;
		if ((int)((StyleLength)(ref width)).keyword != 0 || !(((StyleLength)(ref width)).value == newWidth))
		{
			element.style.width = StyleLength.op_Implicit(newWidth);
		}
	}

	public static void SetHeightIfChanged(VisualElement element, float newHeightPx)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		StyleLength height = element.style.height;
		if ((int)((StyleLength)(ref height)).keyword == 0)
		{
			Length value = ((StyleLength)(ref height)).value;
			if (Mathf.Approximately(((Length)(ref value)).value, newHeightPx))
			{
				return;
			}
		}
		element.style.height = StyleLength.op_Implicit(new Length(newHeightPx, (LengthUnit)0));
	}

	public static void SetTopIfChanged(VisualElement element, float newTopPx)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		StyleLength top = element.style.top;
		if ((int)((StyleLength)(ref top)).keyword == 0)
		{
			Length value = ((StyleLength)(ref top)).value;
			if (Mathf.Approximately(((Length)(ref value)).value, newTopPx))
			{
				return;
			}
		}
		element.style.top = StyleLength.op_Implicit(new Length(newTopPx, (LengthUnit)0));
	}

	public static void SetStyleIfChanged(ref float cacheValue, float newValue, Action<float> setter)
	{
		if (!Mathf.Approximately(cacheValue, newValue))
		{
			setter(newValue);
			cacheValue = newValue;
		}
	}

	public static void SetScaleIfChanged(ref Vector3 cacheScale, Vector3 newScale, VisualElement element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (cacheScale != newScale)
		{
			element.transform.scale = newScale;
			cacheScale = newScale;
		}
	}

	public static string ConvertToHexColor(string decimalColor)
	{
		if (decimalColor.StartsWith("0x") || decimalColor.StartsWith("0X"))
		{
			decimalColor = decimalColor.Substring(2);
		}
		int num = int.Parse(decimalColor, NumberStyles.HexNumber);
		int num2 = (num >> 16) & 0xFF;
		int num3 = (num >> 8) & 0xFF;
		int num4 = num & 0xFF;
		return $"#{num2:X2}{num3:X2}{num4:X2}";
	}

	public static Color ParseColor(string hex)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!hex.StartsWith("#"))
		{
			hex = "#" + hex;
		}
		Color result = default(Color);
		ColorUtility.TryParseHtmlString(hex, ref result);
		return result;
	}

	public static void SetNodePosition(VisualElement m_nodeFinger, VisualElement watchNode)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3;
		if (Screen.height / Screen.width < 2)
		{
			num = ((float)Screen.width - (float)Screen.height / 1480f * 720f) / 2f;
			num3 = (float)Screen.height / 1480f;
		}
		else
		{
			num2 = ((float)Screen.height - (float)Screen.width / 720f * 1480f) / 2f;
			num3 = (float)Screen.width / 720f;
		}
		Rect worldBound = watchNode.worldBound;
		Vector2 center = ((Rect)(ref worldBound)).center;
		float num4 = center.x - num / num3;
		float num5 = center.y - num2 / num3 + 100f;
		if (num4 <= 360f && num5 <= 990f)
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
		}
		else if (num4 > 360f && num5 <= 990f)
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else if (num4 > 360f && num5 > 990f)
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
		}
		else
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		m_nodeFinger.transform.position = Vector2.op_Implicit(new Vector2(num4, num5));
	}

	public static (float x, float y) GetNodePosition(VisualElement watchNode)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3;
		if (Screen.height / Screen.width < 2)
		{
			num = ((float)Screen.width - (float)Screen.height / 1480f * 720f) / 2f;
			num3 = (float)Screen.height / 1480f;
		}
		else
		{
			num2 = ((float)Screen.height - (float)Screen.width / 720f * 1480f) / 2f;
			num3 = (float)Screen.width / 720f;
		}
		Rect worldBound = watchNode.worldBound;
		Vector2 center = ((Rect)(ref worldBound)).center;
		float item = center.x - num / num3;
		float item2 = center.y - num2 / num3 + 100f;
		return (x: item, y: item2);
	}

	public static void InitPlayerInfo(VisualElement head, int HeadId, VisualElement frame, int HeadFrameId, Label lb_level, int Level)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (head != null)
		{
			string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(HeadId).icon;
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
			head.style.backgroundImage = new StyleBackground(val);
		}
		if (frame != null)
		{
			string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(HeadFrameId).icon;
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
			frame.style.backgroundImage = new StyleBackground(val2);
		}
		if (lb_level != null)
		{
			((TextElement)lb_level).text = Level.ToString();
		}
	}

	public static Color ParseHexColor(string hex)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (hex.StartsWith("0x"))
		{
			hex = "#" + hex.Substring(2);
		}
		Color result = default(Color);
		ColorUtility.TryParseHtmlString(hex, ref result);
		return result;
	}

	public static void ShowPopup(VisualElement popup)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		List<StylePropertyName> list = new List<StylePropertyName>
		{
			new StylePropertyName("scale")
		};
		popup.style.transitionProperty = new StyleList<StylePropertyName>(list);
		List<TimeValue> list2 = new List<TimeValue>
		{
			new TimeValue(0.25f, (TimeUnit)0)
		};
		popup.style.transitionDuration = new StyleList<TimeValue>(list2);
		List<EasingFunction> list3 = new List<EasingFunction>
		{
			new EasingFunction((EasingMode)18)
		};
		popup.style.transitionTimingFunction = new StyleList<EasingFunction>(list3);
		popup.style.transitionDelay = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(0f, (TimeUnit)0)
		});
		popup.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(0.8f, 0.8f)));
		popup.schedule.Execute((Action)delegate
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			popup.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(1.1f, 1.1f)));
		}).StartingIn(50L);
		popup.schedule.Execute((Action)delegate
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			popup.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(1f, 1f)));
		}).StartingIn(150L);
	}

	public static void AutoFontSize(TextElement textElement, float initFontSize, float minFontSize, float limitWidth, float limitHeight = 0f)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = initFontSize;
		((VisualElement)textElement).style.fontSize = StyleLength.op_Implicit(num);
		if (limitHeight > 0f)
		{
			Vector2 val = textElement.MeasureTextSize(textElement.text, limitWidth, (MeasureMode)0, 9999f, (MeasureMode)0);
			while (val.y > limitHeight && num > minFontSize)
			{
				num -= 2f;
				((VisualElement)textElement).style.fontSize = StyleLength.op_Implicit(num);
				val = textElement.MeasureTextSize(textElement.text, limitWidth, (MeasureMode)0, 9999f, (MeasureMode)0);
			}
		}
		else
		{
			Vector2 val2 = textElement.MeasureTextSize(textElement.text, 9999f, (MeasureMode)0, 9999f, (MeasureMode)0);
			while (val2.x > limitWidth && num > minFontSize)
			{
				num -= 2f;
				((VisualElement)textElement).style.fontSize = StyleLength.op_Implicit(num);
				val2 = textElement.MeasureTextSize(textElement.text, 9999f, (MeasureMode)0, 9999f, (MeasureMode)0);
			}
		}
	}

	public static bool IsNpc(long roleId)
	{
		return roleId < 100000;
	}

	public static void SetActive(GameObject buffSpriteGO, bool active)
	{
		if (buffSpriteGO.activeSelf != active)
		{
			buffSpriteGO.SetActive(active);
		}
	}

	public static string ReplaceRichTextColor(string text)
	{
		return text.Replace("<color=#64FF26>", "<color=#007008>").Replace("<color=#FF0000>", "<color=#CC0000>");
	}

	public static string CaptureScreenshot()
	{
		string path = $"share_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
		string text = Path.Combine(Application.temporaryCachePath, path);
		Texture2D val = null;
		try
		{
			val = ScreenCapture.CaptureScreenshotAsTexture();
			byte[] bytes = ImageConversion.EncodeToPNG(val);
			File.WriteAllBytes(text, bytes);
			return text;
		}
		finally
		{
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
	}
}
