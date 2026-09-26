using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class zbMonsterPoolMgr
{
	private Dictionary<int, zbMonsterPool> m_pool;

	private Transform m_poolRoot;

	private readonly List<int[]> m_pendingPreHeat = new List<int[]>();

	private float m_preHeatMadeTotal;

	private float m_preHeatMsTotal;

	public bool HasPendingPreHeat => m_pendingPreHeat.Count > 0;

	public int PendingPreHeatTotal
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_pendingPreHeat.Count; i++)
			{
				num += m_pendingPreHeat[i][1];
			}
			return num;
		}
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<int, zbMonsterPool>();
		m_poolRoot = new GameObject("RootMonsterPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public zbMonster GetMonster(int monsterTid)
	{
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		if (recordByKey == null)
		{
			return null;
		}
		if (zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId) == null)
		{
			return null;
		}
		if (m_pool.ContainsKey(monsterTid))
		{
			return m_pool[monsterTid].GetMonster();
		}
		zbMonsterPool zbMonsterPool2 = new zbMonsterPool();
		zbMonsterPool2.Init(monsterTid, m_poolRoot);
		m_pool.Add(monsterTid, zbMonsterPool2);
		return zbMonsterPool2.GetMonster();
	}

	public void PreHeatMonster(int monsterTid, int count)
	{
		if (count > 0)
		{
			zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
			if (recordByKey != null && zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId) != null)
			{
				EnsurePool(monsterTid).PreHeat(count);
			}
		}
	}

	private zbMonsterPool EnsurePool(int monsterTid)
	{
		if (!m_pool.ContainsKey(monsterTid))
		{
			zbMonsterPool zbMonsterPool2 = new zbMonsterPool();
			zbMonsterPool2.Init(monsterTid, m_poolRoot);
			m_pool.Add(monsterTid, zbMonsterPool2);
		}
		return m_pool[monsterTid];
	}

	public bool EnqueuePreHeat(int monsterTid, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		if (recordByKey == null)
		{
			return false;
		}
		if (zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId) == null)
		{
			return false;
		}
		for (int i = 0; i < m_pendingPreHeat.Count; i++)
		{
			if (m_pendingPreHeat[i][0] == monsterTid)
			{
				m_pendingPreHeat[i][1] += count;
				return true;
			}
		}
		m_pendingPreHeat.Add(new int[2] { monsterTid, count });
		return true;
	}

	public int DrainPreHeat(Stopwatch watch, float budgetMs)
	{
		int num = 0;
		while (m_pendingPreHeat.Count > 0 && watch.Elapsed.TotalMilliseconds < (double)budgetMs)
		{
			int[] array = m_pendingPreHeat[0];
			zbMonsterPool obj = EnsurePool(array[0]);
			int count = Math.Min(array[1], 4);
			int idleCount = obj.IdleCount;
			long elapsedTicks = watch.ElapsedTicks;
			obj.PreHeat(count);
			m_preHeatMsTotal += (float)(watch.ElapsedTicks - elapsedTicks) * 1000f / (float)Stopwatch.Frequency;
			int num2 = obj.IdleCount - idleCount;
			num += num2;
			m_preHeatMadeTotal += num2;
			if (num2 == 0)
			{
				zbPerfLog.LogError("[monsterPool] 预热排水零进度，丢弃入队项: tid=" + array[0] + " 剩余=" + array[1]);
				m_pendingPreHeat.RemoveAt(0);
				continue;
			}
			array[1] -= num2;
			if (array[1] <= 0)
			{
				m_pendingPreHeat.RemoveAt(0);
			}
		}
		return num;
	}

	public string PreHeatStatLine()
	{
		if (m_preHeatMadeTotal <= 0f)
		{
			return "0只/0ms";
		}
		float num = m_preHeatMsTotal / m_preHeatMadeTotal;
		return m_preHeatMadeTotal.ToString("0") + "只/" + m_preHeatMsTotal.ToString("0.#") + "ms(" + num.ToString("0.00") + "ms/只)";
	}

	public void Free()
	{
		foreach (zbMonsterPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
		m_pendingPreHeat.Clear();
		m_preHeatMadeTotal = 0f;
		m_preHeatMsTotal = 0f;
	}

	public void UpdateExpand()
	{
		foreach (zbMonsterPool value in m_pool.Values)
		{
			value.UpdateExpand();
		}
	}

	public void ReturnMonster(zbMonster monster)
	{
		if (m_pool.ContainsKey(monster.monsterRecord.id))
		{
			m_pool[monster.monsterRecord.id].ReturnMonster(monster);
		}
	}
}
