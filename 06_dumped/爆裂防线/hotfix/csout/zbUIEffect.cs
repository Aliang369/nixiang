using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class zbUIEffect
{
	private static Dictionary<VisualElement, IVisualElementScheduledItem> s_upDownMap = new Dictionary<VisualElement, IVisualElementScheduledItem>();

	private static Sprite s_starOff;

	private static Sprite s_starOn;

	private static Sprite s_starOver;

	private static Dictionary<VisualElement, IVisualElementScheduledItem> s_swingPauseMap = new Dictionary<VisualElement, IVisualElementScheduledItem>();

	public static void CreateUpDownAction(VisualElement element, float height, float duration, float phase = 0f)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		StopUpDownAction(element);
		float amplitude = height / 2f;
		float num = duration / 2f * 1000f;
		float num2 = duration / 4f * 1000f;
		List<TimeValue> halfDurationList = new List<TimeValue>
		{
			new TimeValue(num, (TimeUnit)1)
		};
		SetupTransition(element, "translate", num2);
		element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(amplitude), 0f));
		bool up = true;
		IVisualElementScheduledItem value = element.schedule.Execute((Action)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(halfDurationList);
			up = !up;
			element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(up ? amplitude : (0f - amplitude)), 0f));
		}).StartingIn((long)num2).Every((long)num + 2);
		s_upDownMap[element] = value;
	}

	public static void StopUpDownAction(VisualElement element)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (s_upDownMap.TryGetValue(element, out var value))
		{
			value.Pause();
			s_upDownMap.Remove(element);
		}
		ClearTransition(element);
		element.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(0f), 0f));
	}

	public static void CreateScaleAction(VisualElement element, float minScale, float maxScale, float duration)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		float num = duration / 2f * 1000f;
		float num2 = num * Rand.Range(0.5f, 1f);
		bool toMax = Rand.Range(0f, 1f) < 0.5f;
		List<TimeValue> halfDurationList = new List<TimeValue>
		{
			new TimeValue(num, (TimeUnit)1)
		};
		SetupTransition(element, "scale", num2);
		element.style.scale = StyleScale.op_Implicit(new Scale(Vector2.one * (toMax ? maxScale : minScale)));
		element.schedule.Execute((Action)delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			if ((int)element.resolvedStyle.display == 0)
			{
				element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(halfDurationList);
				toMax = !toMax;
				element.style.scale = StyleScale.op_Implicit(new Scale(Vector2.one * (toMax ? maxScale : minScale)));
			}
		}).StartingIn((long)num2).Every((long)num + 2);
	}

	public static void CreateScaleActionOnce(VisualElement element, float maxScale, float duration)
	{
		float startTime = Time.realtimeSinceStartup;
		IVisualElementScheduledItem item = null;
		item = element.schedule.Execute((Action)delegate
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / duration);
			if (num >= 1f)
			{
				element.style.scale = StyleScale.op_Implicit(Vector2.one);
				item.Pause();
			}
			else if ((int)element.resolvedStyle.display == 0)
			{
				float num2 = 1f - Mathf.Abs(num * 2f - 1f);
				num2 = Mathf.SmoothStep(0f, 1f, num2);
				float num3 = Mathf.Lerp(1f, maxScale, num2);
				element.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num3, num3)));
			}
		}).Every(0L);
	}

	public static void CreateBreathingLightEffect(VisualElement element, float minAlpha, float maxAlpha, float duration)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		float num = duration / 2f * 1000f;
		float num2 = num * Rand.Range(0.5f, 1f);
		bool toMax = Rand.Range(0f, 1f) < 0.5f;
		List<TimeValue> halfDurationList = new List<TimeValue>
		{
			new TimeValue(num, (TimeUnit)1)
		};
		SetupTransition(element, "opacity", num2);
		element.style.opacity = StyleFloat.op_Implicit(toMax ? maxAlpha : minAlpha);
		element.schedule.Execute((Action)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(halfDurationList);
			toMax = !toMax;
			element.style.opacity = StyleFloat.op_Implicit(toMax ? maxAlpha : minAlpha);
		}).StartingIn((long)num2).Every((long)num + 2);
	}

	public static void CreateRotateAction(VisualElement element, float duration, bool clockwise = true)
	{
		float speed = 360f / duration;
		element.schedule.Execute((Action)delegate
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			float num = Time.realtimeSinceStartup * speed % 360f;
			if (!clockwise)
			{
				num = 0f - num;
			}
			element.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(num)));
		}).Every(0L);
	}

	public static void BlinkTimes(VisualElement element, float minAlpha = 0.3f, float maxAlpha = 1f, int intervalMs = 200)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		int flips = 0;
		bool isBright = false;
		element.style.opacity = StyleFloat.op_Implicit(minAlpha);
		element.schedule.Execute((Action)delegate
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			isBright = !isBright;
			element.style.opacity = StyleFloat.op_Implicit(isBright ? maxAlpha : minAlpha);
			flips++;
			if (flips >= 12)
			{
				element.style.opacity = StyleFloat.op_Implicit(maxAlpha);
			}
		}).Every((long)intervalMs).Until((Func<bool>)(() => flips >= 12));
	}

	public static void FadeTree(VisualElement root, float startOpacity, float endOpacity, float duration = 0.3f, float delayStep = 0f)
	{
		int index = 0;
		FadeRecursive(root, startOpacity, endOpacity, duration, delayStep, ref index);
	}

	private static void FadeRecursive(VisualElement ve, float startOpacity, float endOpacity, float duration, float delayStep, ref int index)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float num = delayStep * (float)index;
		index++;
		ve.style.opacity = StyleFloat.op_Implicit(startOpacity);
		ve.schedule.Execute((Action)delegate
		{
			ve.experimental.animation.Start(startOpacity, endOpacity, (int)(duration * 1000f), (Action<VisualElement, float>)delegate(VisualElement e, float value)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				ve.style.opacity = StyleFloat.op_Implicit(value);
			});
		}).ExecuteLater((long)(num * 1000f));
		foreach (VisualElement item in ve.Children())
		{
			FadeRecursive(item, startOpacity, endOpacity, duration, delayStep, ref index);
		}
	}

	public static void SetStars(VisualElement[] m_stars, int Star, bool moreVis = true)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)s_starOn == (Object)null)
		{
			s_starOn = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/suitGun/star_1.png");
		}
		if ((Object)(object)s_starOff == (Object)null)
		{
			s_starOff = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/suitGun/star_0.png");
		}
		if ((Object)(object)s_starOver == (Object)null)
		{
			s_starOver = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/suitGun/star_2.png");
		}
		for (int i = 0; i < 5; i++)
		{
			if (i < Star)
			{
				m_stars[i].style.backgroundImage = new StyleBackground(s_starOn);
			}
			else
			{
				m_stars[i].style.backgroundImage = new StyleBackground(s_starOff);
			}
			bool flag = (!moreVis && i < Star) | moreVis;
			m_stars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
		for (int j = 5; j < 10; j++)
		{
			if (j < Star)
			{
				m_stars[j - 5].style.backgroundImage = new StyleBackground(s_starOver);
			}
		}
	}

	public static void CreateSwingPauseAction(VisualElement element, float angle = 12f, int swingCount = 4, float swingDuration = 0.2f, float pauseDuration = 1f)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		StopSwingPauseAction(element);
		element.style.transformOrigin = StyleTransformOrigin.op_Implicit(new TransformOrigin(new Length(50f, (LengthUnit)1), new Length(100f, (LengthUnit)1), 0f));
		float num = swingDuration / 2f * 1000f;
		int swingTicks = swingCount * 2;
		int num2 = Mathf.Max(1, Mathf.CeilToInt(pauseDuration / (swingDuration / 2f)));
		int totalTicks = swingTicks + 1 + num2;
		SetupTransition(element, "rotate", num);
		element.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(0f)));
		int tick = 0;
		bool left = true;
		IVisualElementScheduledItem value = element.schedule.Execute((Action)delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			if ((int)element.resolvedStyle.display == 0)
			{
				int num3 = tick % totalTicks;
				tick++;
				if (num3 < swingTicks)
				{
					left = !left;
					element.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(left ? (0f - angle) : angle)));
				}
				else if (num3 == swingTicks)
				{
					element.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(0f)));
				}
			}
		}).Every((long)num + 2);
		s_swingPauseMap[element] = value;
	}

	public static void StopSwingPauseAction(VisualElement element)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (s_swingPauseMap.TryGetValue(element, out var value))
		{
			value.Pause();
			s_swingPauseMap.Remove(element);
		}
		ClearTransition(element);
		element.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(0f)));
	}

	private static void SetupTransition(VisualElement element, string property, float firstDurationMs)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		element.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>
		{
			new StylePropertyName(property)
		});
		element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(firstDurationMs, (TimeUnit)1)
		});
		element.style.transitionTimingFunction = StyleList<EasingFunction>.op_Implicit(new List<EasingFunction>
		{
			new EasingFunction((EasingMode)7)
		});
	}

	private static void ClearTransition(VisualElement element)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		element.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>());
		element.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(0f, (TimeUnit)1)
		});
	}
}
