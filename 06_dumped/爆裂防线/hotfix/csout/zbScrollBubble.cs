using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbScrollBubble
{
	private const int MAX_BUBBLES = 15;

	private const float SCROLL_SPEED = 156f;

	private VisualElement m_eleContainer;

	private Label m_lbMessage;

	private bool m_bPlaying;

	private bool m_bPaused;

	private Queue<string> bubbleQueue;

	private Coroutine m_scrollCoroutine;

	public zbScrollBubble()
	{
		m_bPlaying = false;
		m_bPaused = false;
		bubbleQueue = new Queue<string>();
	}

	public void OnEnable(VisualElement eleParent)
	{
		m_eleContainer = UQueryExtensions.Q<VisualElement>(eleParent, "sroll_container", (string)null);
		m_lbMessage = UQueryExtensions.Q<Label>(eleParent, "scroll_label", (string)null);
		m_eleContainer.visible = false;
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)m_lbMessage);
	}

	public void PushScrollBubble(string message)
	{
		if (!m_bPaused)
		{
			if (zbUIMgr.Instance.IsShow(EWndId.Vip) || zbUIMgr.Instance.IsShow(EWndId.Bar))
			{
				((MonoBehaviour)zbUIMgr.Instance).StartCoroutine(DelayPush(message, 4f));
			}
			else
			{
				EnqueueMessage(message);
			}
		}
	}

	private IEnumerator DelayPush(string message, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		EnqueueMessage(message);
	}

	private void EnqueueMessage(string message)
	{
		if (bubbleQueue.Count >= 15)
		{
			bubbleQueue.Dequeue();
		}
		bubbleQueue.Enqueue(message);
		if (!m_bPlaying)
		{
			StartNextBubble();
		}
	}

	private void StartNextBubble()
	{
		if (bubbleQueue.Count > 0)
		{
			m_bPlaying = true;
			m_eleContainer.visible = true;
			string message = bubbleQueue.Dequeue();
			m_scrollCoroutine = ((MonoBehaviour)zbUIMgr.Instance).StartCoroutine(ScrollBubble(message));
		}
		else
		{
			m_bPlaying = false;
			m_eleContainer.visible = false;
		}
	}

	private IEnumerator ScrollBubble(string message)
	{
		((TextElement)m_lbMessage).text = message;
		float x = ((TextElement)m_lbMessage).MeasureTextSize(message, 0f, (MeasureMode)0, 0f, (MeasureMode)0).x;
		((VisualElement)m_lbMessage).style.width = StyleLength.op_Implicit(x);
		float width = m_eleContainer.resolvedStyle.width;
		float startX = width;
		float endX = 0f - x;
		float num = startX - endX;
		float duration = num / 156f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			if (!m_bPaused)
			{
				elapsed += Time.unscaledDeltaTime;
				float num2 = elapsed / duration;
				((VisualElement)m_lbMessage).style.left = StyleLength.op_Implicit(Mathf.Lerp(startX, endX, num2));
			}
			yield return null;
		}
		StartNextBubble();
	}

	public void Pause()
	{
		m_bPaused = true;
	}

	public void Resume()
	{
		m_bPaused = false;
	}

	public void Stop()
	{
		if (m_scrollCoroutine != null)
		{
			((MonoBehaviour)zbUIMgr.Instance).StopCoroutine(m_scrollCoroutine);
			m_scrollCoroutine = null;
		}
		bubbleQueue.Clear();
		m_bPlaying = false;
		m_bPaused = false;
		if (m_eleContainer != null)
		{
			m_eleContainer.visible = false;
		}
	}
}
