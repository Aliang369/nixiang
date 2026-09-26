using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBubbleWnd : zbBaseWnd
{
	private struct BubbleMsg(string msg, int code = -1)
	{
		public string msg = msg;

		public int code = code;
	}

	private const int maxBubbles = 3;

	private const float startY = 600f;

	private const float baseTargetY = 300f;

	private const float bubbleHeight = 50f;

	private const float spacing = 10f;

	private const float displayDuration = 3f;

	private const float fadeDuration = 0.5f;

	private float lastShowTime;

	private const float showInterval = 0.2f;

	private List<VisualElement> bubblePool = new List<VisualElement>();

	private List<VisualElement> activeBubbles = new List<VisualElement>();

	private Dictionary<VisualElement, Coroutine> bubbleCoroutines = new Dictionary<VisualElement, Coroutine>();

	private Queue<BubbleMsg> messageQueue = new Queue<BubbleMsg>();

	protected override void OnEnable()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root_top", (string)null);
		messageQueue.Clear();
		bubblePool.Clear();
		activeBubbles.Clear();
		bubbleCoroutines.Clear();
		for (int i = 0; i < 3; i++)
		{
			VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, $"bubble{i + 1}", (string)null);
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			val2.style.opacity = StyleFloat.op_Implicit(0f);
			val2.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			bubblePool.Add(val2);
		}
	}

	protected override void OnDisable()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisable();
		foreach (Coroutine value in bubbleCoroutines.Values)
		{
			if (value != null)
			{
				((MonoBehaviour)this).StopCoroutine(value);
			}
		}
		messageQueue.Clear();
		bubbleCoroutines.Clear();
		foreach (VisualElement activeBubble in activeBubbles)
		{
			activeBubble.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			activeBubble.style.opacity = StyleFloat.op_Implicit(0f);
			bubblePool.Add(activeBubble);
		}
		activeBubbles.Clear();
	}

	public void BubbleInfo(string msg, int code = -1)
	{
		messageQueue.Enqueue(new BubbleMsg(msg, code));
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (messageQueue.Count != 0 && Time.unscaledTime - lastShowTime >= 0.2f)
		{
			BubbleMsg bubbleMsg = messageQueue.Dequeue();
			ShowBubbleMsg(bubbleMsg.msg, bubbleMsg.code);
			lastShowTime = Time.unscaledTime;
		}
	}

	private void ShowBubbleMsg(string msg, int code)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (activeBubbles.Count >= 3)
		{
			VisualElement val = activeBubbles[0];
			if (bubbleCoroutines.TryGetValue(val, out var value))
			{
				if (value != null)
				{
					((MonoBehaviour)this).StopCoroutine(value);
				}
				bubbleCoroutines.Remove(val);
			}
			val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			val.style.opacity = StyleFloat.op_Implicit(0f);
			activeBubbles.RemoveAt(0);
			bubblePool.Add(val);
			UpdateBubblePositions();
		}
		if (bubblePool.Count != 0)
		{
			VisualElement val2 = bubblePool[0];
			bubblePool.RemoveAt(0);
			((TextElement)UQueryExtensions.Q<Label>(val2, "errormsg", (string)null)).text = msg;
			Label val3 = UQueryExtensions.Q<Label>(val2, "errorcode", (string)null);
			if (code != -1)
			{
				((VisualElement)val3).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)val3).text = code.ToString();
			}
			else
			{
				((VisualElement)val3).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			val2.style.opacity = StyleFloat.op_Implicit(0f);
			val2.style.top = StyleLength.op_Implicit(600f);
			float toY = 300f + (float)activeBubbles.Count * 60f;
			activeBubbles.Add(val2);
			((MonoBehaviour)this).StartCoroutine(FadeTo(val2, 0f, 1f, 0.5f));
			((MonoBehaviour)this).StartCoroutine(MoveToY(val2, 600f, toY, 0.5f));
			Coroutine value2 = ((MonoBehaviour)this).StartCoroutine(HideBubbleAfterDelay(val2, 3f));
			bubbleCoroutines[val2] = value2;
		}
	}

	private IEnumerator HideBubbleAfterDelay(VisualElement bubble, float delay)
	{
		yield return (object)new WaitForSecondsRealtime(delay);
		yield return FadeTo(bubble, 1f, 0f, 0.5f);
		bubble.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		activeBubbles.Remove(bubble);
		bubblePool.Add(bubble);
		bubbleCoroutines.Remove(bubble);
		UpdateBubblePositions();
	}

	private void UpdateBubblePositions()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < activeBubbles.Count; i++)
		{
			VisualElement val = activeBubbles[i];
			StyleLength top = val.style.top;
			Length value = ((StyleLength)(ref top)).value;
			float value2 = ((Length)(ref value)).value;
			float num = 300f + (float)i * 60f;
			if (Mathf.Abs(value2 - num) > 0.1f)
			{
				((MonoBehaviour)this).StartCoroutine(MoveToY(val, value2, num, 0.5f));
			}
		}
	}

	private IEnumerator FadeTo(VisualElement element, float from, float to, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			element.style.opacity = StyleFloat.op_Implicit(Mathf.Lerp(from, to, elapsed / duration));
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		element.style.opacity = StyleFloat.op_Implicit(to);
	}

	private IEnumerator MoveToY(VisualElement element, float fromY, float toY, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			float num = Mathf.Lerp(fromY, toY, elapsed / duration);
			element.style.top = StyleLength.op_Implicit(num);
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		element.style.top = StyleLength.op_Implicit(toY);
	}

	public void BubbleErrorCode(string msg, int code)
	{
		string key = "lang.EErrorCode." + code;
		BubbleInfo(zbUtil.Language(key), code);
	}
}
