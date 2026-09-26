using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBubblePool
{
	private zbCombatData m_combatData;

	private int m_onceAllocSize = 10;

	private List<zbBubbleNumber> m_pool;

	private List<zbBubbleNumber> m_activeBubbles;

	private zbBubbleRequestQueue m_requestQueue = new zbBubbleRequestQueue();

	private VisualElement m_rootBubble;

	public Vector2 m_rootBubbleWorldBoundPos;

	public Vector2 m_worldToPanelScale;

	public Vector2 m_worldToPanelOffset;

	public void Init(zbCombatData combatData)
	{
		m_combatData = combatData;
		m_pool = new List<zbBubbleNumber>();
		m_activeBubbles = new List<zbBubbleNumber>();
		zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		m_rootBubble = UQueryExtensions.Q<VisualElement>(zbCombatWnd2.GetRootElement(), "rootBubble", (string)null);
		m_requestQueue = new zbBubbleRequestQueue();
		m_requestQueue.Init(this, combatData);
		UpdateWorldToPanelCache();
	}

	public void Free()
	{
		ReturnAllBubbles();
		m_pool.Clear();
		m_activeBubbles.Clear();
		m_requestQueue.Clear();
	}

	public zbBubbleNumber GetBubble()
	{
		int maxBubbleActive = m_combatData.GetMaxBubbleActive();
		if (m_activeBubbles.Count >= maxBubbleActive)
		{
			float num = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
			float num2 = Mathf.Lerp(1.2f, 2f, Mathf.Clamp01((num - 15f) / 45f));
			int num3 = Mathf.RoundToInt((float)maxBubbleActive * num2);
			if (m_activeBubbles.Count < num3)
			{
				int num4 = m_activeBubbles.Count - maxBubbleActive;
				int num5 = Mathf.Max(5, num4 / 2);
				num5 = Mathf.Min(num5, m_activeBubbles.Count);
				for (int i = 0; i < num5; i++)
				{
					m_activeBubbles[i].ForceAccelerateFadeOut();
				}
			}
			else
			{
				int num6 = Mathf.Min(5, m_activeBubbles.Count);
				for (int j = 0; j < num6; j++)
				{
					zbBubbleNumber bubble = m_activeBubbles[0];
					ReturnBubble(bubble);
				}
			}
		}
		if (m_pool.Count == 0)
		{
			ExpandPool(m_onceAllocSize);
		}
		if (m_pool.Count > 0)
		{
			int index = m_pool.Count - 1;
			zbBubbleNumber zbBubbleNumber2 = m_pool[index];
			m_pool.RemoveAt(index);
			if (!zbBubbleNumber2.IsActive())
			{
				zbBubbleNumber2.SetActive(active: true);
			}
			zbBubbleNumber2.activeIndex = m_activeBubbles.Count;
			m_activeBubbles.Add(zbBubbleNumber2);
			return zbBubbleNumber2;
		}
		return null;
	}

	public void ReturnBubble(zbBubbleNumber bubble)
	{
		if (!bubble.IsActive())
		{
			return;
		}
		bubble.SetActive(active: false);
		int activeIndex = bubble.activeIndex;
		int num = m_activeBubbles.Count - 1;
		if (activeIndex >= 0 && activeIndex <= num && m_activeBubbles[activeIndex] == bubble)
		{
			if (activeIndex != num)
			{
				zbBubbleNumber zbBubbleNumber2 = m_activeBubbles[num];
				m_activeBubbles[activeIndex] = zbBubbleNumber2;
				zbBubbleNumber2.activeIndex = activeIndex;
			}
			m_activeBubbles.RemoveAt(num);
		}
		bubble.activeIndex = -1;
		m_pool.Add(bubble);
	}

	public void ReturnAllBubbles()
	{
		foreach (zbBubbleNumber activeBubble in m_activeBubbles)
		{
			activeBubble.SetActive(active: false);
			activeBubble.activeIndex = -1;
			m_pool.Add(activeBubble);
		}
		m_activeBubbles.Clear();
	}

	private void ExpandPool(int newSize)
	{
		int num = newSize - m_pool.Count;
		for (int i = 0; i < num; i++)
		{
			zbBubbleNumber zbBubbleNumber2 = new zbBubbleNumber();
			zbBubbleNumber2.Init(m_rootBubble);
			zbBubbleNumber2.SetActive(active: false);
			m_pool.Add(zbBubbleNumber2);
		}
	}

	public void AddRequest(Vector3 pos, ECombatBubble type, long num)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_requestQueue.Enqueue(pos, type, num);
	}

	public void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (m_rootBubbleWorldBoundPos == Vector2.zero || zbIntervalTimer.Instance.IsOneSecondPassed())
		{
			Rect worldBound = m_rootBubble.worldBound;
			m_rootBubbleWorldBoundPos = ((Rect)(ref worldBound)).position;
			UpdateWorldToPanelCache();
		}
		m_requestQueue.Process();
		for (int num = m_activeBubbles.Count - 1; num >= 0; num--)
		{
			m_activeBubbles[num].Update();
		}
	}

	public void UpdateWorldToPanelCache()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		Vector3 right = Vector3.right;
		Vector3 up = Vector3.up;
		Vector2 val = RuntimePanelUtils.CameraTransformWorldToPanel(m_rootBubble.panel, zero, m_combatData.mainCamera);
		Vector2 val2 = RuntimePanelUtils.CameraTransformWorldToPanel(m_rootBubble.panel, right, m_combatData.mainCamera);
		Vector2 val3 = RuntimePanelUtils.CameraTransformWorldToPanel(m_rootBubble.panel, up, m_combatData.mainCamera);
		m_worldToPanelScale.x = val2.x - val.x;
		m_worldToPanelScale.y = val3.y - val.y;
		m_worldToPanelOffset = val;
	}
}
