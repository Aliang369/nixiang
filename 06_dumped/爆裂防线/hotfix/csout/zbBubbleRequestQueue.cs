using System.Collections.Generic;
using UnityEngine;

public class zbBubbleRequestQueue
{
	private zbBubblePool m_pool;

	private zbCombatData m_combatData;

	private Queue<zbBubbleRequest> m_queue = new Queue<zbBubbleRequest>();

	private int m_maxConsumePerFrame = 30;

	private const int MIN_CONSUME = 1;

	private const int MAX_CONSUME = 30;

	private const int MAX_QUEUE_LENGTH = 200;

	private const float MAX_BUBBLE_DELAY = 1f;

	private float m_targetFPS = 60f;

	private float m_smoothedFPS = 60f;

	private float m_fpsSmoothing = 0.1f;

	public int Count => m_queue.Count;

	public void Init(zbBubblePool pool, zbCombatData combatData)
	{
		m_pool = pool;
		m_combatData = combatData;
	}

	public void Clear()
	{
		m_queue.Clear();
	}

	public void Enqueue(Vector3 pos, ECombatBubble type, long num)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (m_queue.Count >= 200)
		{
			m_queue.Dequeue();
		}
		m_queue.Enqueue(new zbBubbleRequest(pos, type, num));
	}

	public void Process()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		UpdateFPS();
		AdjustConsumePerFrame();
		int num = Mathf.Min(m_queue.Count, m_maxConsumePerFrame);
		for (int i = 0; i < num; i++)
		{
			zbBubbleRequest zbBubbleRequest2 = m_queue.Peek();
			if (Time.time - zbBubbleRequest2.EnqueueTime > 1f)
			{
				m_queue.Dequeue();
				continue;
			}
			m_queue.Dequeue();
			m_pool.GetBubble()?.Show(m_combatData, zbBubbleRequest2.WorldPos, zbBubbleRequest2.BubbleType, zbBubbleRequest2.Num);
		}
	}

	private void UpdateFPS()
	{
		float num = 1f / Time.unscaledDeltaTime;
		m_smoothedFPS = Mathf.Lerp(m_smoothedFPS, num, m_fpsSmoothing);
	}

	private void AdjustConsumePerFrame()
	{
		float num = m_smoothedFPS / m_targetFPS;
		m_maxConsumePerFrame = Mathf.Clamp(Mathf.RoundToInt(30f * num), 1, 30);
		if (m_queue.Count > 100)
		{
			int num2 = m_queue.Count / 20;
			m_maxConsumePerFrame = Mathf.Min(m_maxConsumePerFrame + num2, 60);
		}
	}
}
