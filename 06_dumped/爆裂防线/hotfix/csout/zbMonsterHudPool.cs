using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterHudPool
{
	private zbCombatData m_combatData;

	private int m_onceAllocSize = 1;

	private LinkedList<zbMonsterHudProgress> m_pool;

	private LinkedList<zbMonsterHudProgress> m_activeHuds;

	private VisualElement m_rootHud;

	public Vector2 m_rootHudWorldBoundPos;

	public void Init(zbCombatData combatData)
	{
		m_combatData = combatData;
		m_pool = new LinkedList<zbMonsterHudProgress>();
		m_activeHuds = new LinkedList<zbMonsterHudProgress>();
		zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		m_rootHud = UQueryExtensions.Q<VisualElement>(zbCombatWnd2.GetRootElement(), "rootHud", (string)null);
		CheckHudRootWorldBoundPosition();
	}

	public void Free()
	{
		ReturnAllHuds();
		m_pool.Clear();
		m_activeHuds.Clear();
	}

	public zbMonsterHudProgress GetHud()
	{
		zbMonsterHudProgress zbMonsterHudProgress2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(m_onceAllocSize);
		}
		if (m_pool.Count > 0)
		{
			zbMonsterHudProgress2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			m_activeHuds.AddLast(zbMonsterHudProgress2);
			return zbMonsterHudProgress2;
		}
		return null;
	}

	public void ReturnHud(zbMonsterHudProgress hud)
	{
		if (hud.IsActive())
		{
			hud.SetActive(active: false);
			m_activeHuds.Remove(hud);
			m_pool.AddLast(hud);
		}
	}

	public void ReturnAllHuds()
	{
		foreach (zbMonsterHudProgress activeHud in m_activeHuds)
		{
			activeHud.SetActive(active: false);
			m_pool.AddLast(activeHud);
		}
		m_activeHuds.Clear();
	}

	public void ExpandPool(int newSize)
	{
		int num = newSize - m_pool.Count;
		for (int i = 0; i < num; i++)
		{
			zbMonsterHudProgress zbMonsterHudProgress2 = new zbMonsterHudProgress();
			zbMonsterHudProgress2.Init(m_rootHud);
			zbMonsterHudProgress2.SetActive(active: false);
			m_pool.AddLast(zbMonsterHudProgress2);
		}
	}

	public void Update()
	{
		LinkedListNode<zbMonsterHudProgress> linkedListNode = m_activeHuds.First;
		while (linkedListNode != null)
		{
			LinkedListNode<zbMonsterHudProgress> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode.Next;
			linkedListNode2.Value.Update();
		}
	}

	public void CheckHudRootWorldBoundPosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Rect worldBound = m_rootHud.worldBound;
		m_rootHudWorldBoundPos = ((Rect)(ref worldBound)).position;
		int frameCount = 0;
		m_rootHud.schedule.Execute((Action)delegate
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			frameCount++;
			if (frameCount >= 3)
			{
				zbMonsterHudPool obj = this;
				Rect worldBound2 = m_rootHud.worldBound;
				obj.m_rootHudWorldBoundPos = ((Rect)(ref worldBound2)).position;
			}
		}).Every(1L).Until((Func<bool>)(() => frameCount >= 3));
	}
}
