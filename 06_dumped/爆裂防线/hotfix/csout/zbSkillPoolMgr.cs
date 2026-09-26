using System.Collections.Generic;
using UnityEngine;

public class zbSkillPoolMgr
{
	public Dictionary<string, zbSkillPool> m_pool;

	private Transform m_poolRoot;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "技能弹池活跃数";

	private const string kCounterPeak = "技能弹池活跃峰值";

	private readonly Dictionary<int, int> m_activeSubSkillCount = new Dictionary<int, int>();

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("技能弹池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("技能弹池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("技能弹池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<string, zbSkillPool>();
		m_poolRoot = new GameObject("RootSkillPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
		m_activeSubSkillCount.Clear();
	}

	public zbSkillBase GetSkill(string prefabPath)
	{
		if (prefabPath.Length == 0)
		{
			return null;
		}
		zbSkillBase skill;
		if (m_pool.ContainsKey(prefabPath))
		{
			skill = m_pool[prefabPath].GetSkill();
		}
		else
		{
			zbSkillPool zbSkillPool2 = new zbSkillPool();
			zbSkillPool2.Init(prefabPath, m_poolRoot);
			m_pool.Add(prefabPath, zbSkillPool2);
			skill = zbSkillPool2.GetSkill();
		}
		if ((Object)(object)skill != (Object)null)
		{
			OnPoolGet();
		}
		return skill;
	}

	public void PreHeatSkeletonData(string prefabPath)
	{
		if (prefabPath.Length != 0)
		{
			EnsurePool(prefabPath).PreHeatSkeletonData();
		}
	}

	public void PreHeatPool(string prefabPath, int count)
	{
		if (prefabPath.Length != 0)
		{
			EnsurePool(prefabPath).PreHeat(count);
		}
	}

	private zbSkillPool EnsurePool(string prefabPath)
	{
		if (!m_pool.ContainsKey(prefabPath))
		{
			zbSkillPool zbSkillPool2 = new zbSkillPool();
			zbSkillPool2.Init(prefabPath, m_poolRoot);
			m_pool.Add(prefabPath, zbSkillPool2);
		}
		return m_pool[prefabPath];
	}

	public bool HasActiveSubSkill(int subSkillId)
	{
		if (m_activeSubSkillCount.TryGetValue(subSkillId, out var value))
		{
			return value > 0;
		}
		return false;
	}

	public void RegisterActiveSubSkill(int subSkillId)
	{
		m_activeSubSkillCount.TryGetValue(subSkillId, out var value);
		m_activeSubSkillCount[subSkillId] = value + 1;
	}

	private void UnregisterActiveSubSkill(int subSkillId)
	{
		if (m_activeSubSkillCount.TryGetValue(subSkillId, out var value) && value > 0)
		{
			m_activeSubSkillCount[subSkillId] = value - 1;
		}
	}

	public void ReturnSkill(string prefabPath, zbSkillBase skill)
	{
		if (m_pool.ContainsKey(prefabPath))
		{
			if (skill.isSubSkill && skill.skillSubRecord != null && (Object)(object)skill.skillGO != (Object)null && skill.skillGO.activeSelf)
			{
				UnregisterActiveSubSkill(skill.skillSubRecord.id);
			}
			m_pool[prefabPath].ReturnSkill(skill);
			OnPoolReturn();
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		foreach (zbSkillPool value in m_pool.Values)
		{
			value.CollectActiveSpineZ(results);
		}
	}

	public void Free()
	{
		foreach (zbSkillPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
		m_activeSubSkillCount.Clear();
	}

	public zbSkillPool FindSkillPool(int skillSubId)
	{
		string skillSubPrefab = stSkillSubPrefab.GetSkillSubPrefab(skillSubId);
		if (skillSubPrefab != "" && m_pool.ContainsKey(skillSubPrefab))
		{
			return m_pool[skillSubPrefab];
		}
		return null;
	}

	public zbSkillBase FindSkill(int skillSubId)
	{
		foreach (KeyValuePair<string, zbSkillPool> item in m_pool)
		{
			foreach (zbSkillBase activeSkill in item.Value.m_activeSkills)
			{
				if (activeSkill.isSubSkill && activeSkill.skillSubRecord.id == skillSubId)
				{
					return activeSkill;
				}
			}
		}
		return null;
	}

	public zbSkillPool FindSkillString(string prefabPath)
	{
		if (m_pool.ContainsKey(prefabPath))
		{
			return m_pool[prefabPath];
		}
		return null;
	}
}
