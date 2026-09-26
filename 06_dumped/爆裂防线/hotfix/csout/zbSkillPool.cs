using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbSkillPool
{
	private string m_prefabPath;

	private LinkedList<zbSkillBase> m_pool;

	public LinkedList<zbSkillBase> m_activeSkills;

	private bool m_skeletonDataPrepared;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	public void Init(string prefabPath, Transform poolRoot)
	{
		m_prefabPath = prefabPath;
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbSkillBase>();
		m_activeSkills = new LinkedList<zbSkillBase>();
	}

	public zbSkillBase GetSkill()
	{
		zbSkillBase zbSkillBase2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch(), initialActive: false);
		}
		if (m_pool.Count > 0)
		{
			zbSkillBase2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			GameObject val = (((Object)(object)zbSkillBase2.skillGO != (Object)null) ? zbSkillBase2.skillGO : ((Component)zbSkillBase2).gameObject);
			if (!val.activeSelf)
			{
				val.SetActive(true);
			}
			if ((Object)(object)zbSkillBase2.skillGO == (Object)null)
			{
				zbSkillBase2.skillGO = val;
			}
			m_activeSkills.AddLast(zbSkillBase2);
			return zbSkillBase2;
		}
		return null;
	}

	public void ReturnSkill(zbSkillBase skill)
	{
		if (skill.skillGO.activeSelf)
		{
			skill.skillGO.SetActive(false);
			m_activeSkills.Remove(skill);
			m_pool.AddLast(skill);
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbSkillBase activeSkill in m_activeSkills)
		{
			if (!((Object)(object)activeSkill == (Object)null) && !activeSkill.m_spineGpuTakenOver && !((Object)(object)activeSkill.m_skeletonAnimation == (Object)null))
			{
				Transform transform = ((Component)activeSkill.m_skeletonAnimation).transform;
				if (!((Object)(object)transform == (Object)null) && ((Component)transform).gameObject.activeInHierarchy)
				{
					results.Add(transform.position.z);
				}
			}
		}
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeSkills.Clear();
	}

	public void PreHeatSkeletonData()
	{
		if (m_skeletonDataPrepared)
		{
			return;
		}
		m_skeletonDataPrepared = true;
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>(m_prefabPath);
		if (!((Object)(object)val == (Object)null))
		{
			SkeletonRenderer componentInChildren = val.GetComponentInChildren<SkeletonRenderer>(true);
			if ((Object)(object)componentInChildren != (Object)null && (Object)(object)componentInChildren.skeletonDataAsset != (Object)null)
			{
				componentInChildren.skeletonDataAsset.GetSkeletonData(true);
			}
		}
	}

	public void PreHeat(int count)
	{
		if (count > 0)
		{
			PreHeatSkeletonData();
			ExpandPool(count, initialActive: false);
		}
	}

	private int CalcExpandBatch()
	{
		int num = (m_pool.Count + m_activeSkills.Count + 1) / 2;
		if (num < 1)
		{
			num = 1;
		}
		if (num > 16)
		{
			num = 16;
		}
		return num;
	}

	private void ExpandPool(int count, bool initialActive = true)
	{
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>(m_prefabPath);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if (!m_skeletonDataPrepared)
		{
			SkeletonRenderer componentInChildren = val.GetComponentInChildren<SkeletonRenderer>(true);
			if ((Object)(object)componentInChildren != (Object)null && (Object)(object)componentInChildren.skeletonDataAsset != (Object)null)
			{
				componentInChildren.skeletonDataAsset.GetSkeletonData(true);
			}
			m_skeletonDataPrepared = true;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbSkillBase component = val2.GetComponent<zbSkillBase>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			if (!initialActive)
			{
				val2.SetActive(false);
			}
			m_pool.AddLast(component);
		}
	}
}
