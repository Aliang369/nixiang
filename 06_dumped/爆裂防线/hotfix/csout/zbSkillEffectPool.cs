using System.Collections.Generic;
using UnityEngine;

public class zbSkillEffectPool
{
	private LinkedList<zbSkillEffect> m_pool;

	private LinkedList<zbSkillEffect> m_activeeffects;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	private int effectSkillId;

	public void Init(int _effectSkillId, Transform poolRoot)
	{
		effectSkillId = _effectSkillId;
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbSkillEffect>();
		m_activeeffects = new LinkedList<zbSkillEffect>();
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeeffects.Clear();
	}

	public zbSkillEffect GetEffect()
	{
		zbSkillEffect zbSkillEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbSkillEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbSkillEffect2).gameObject.activeSelf)
			{
				((Component)zbSkillEffect2).gameObject.SetActive(true);
			}
			m_activeeffects.AddLast(zbSkillEffect2);
			return zbSkillEffect2;
		}
		return null;
	}

	public void ReturnEffect(zbSkillEffect effect)
	{
		if (!((Object)(object)effect == (Object)null))
		{
			((Component)effect).gameObject.SetActive(false);
			m_activeeffects.Remove(effect);
			m_pool.AddLast(effect);
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbSkillEffect activeeffect in m_activeeffects)
		{
			if (!((Object)(object)activeeffect == (Object)null) && !activeeffect.m_spineGpuTakenOver && !((Object)(object)activeeffect.skeletonAnimation == (Object)null))
			{
				Transform transform = ((Component)activeeffect.skeletonAnimation).transform;
				if (!((Object)(object)transform == (Object)null) && ((Component)transform).gameObject.activeInHierarchy)
				{
					results.Add(transform.position.z);
				}
			}
		}
	}

	private int CalcExpandBatch()
	{
		int num = (m_pool.Count + m_activeeffects.Count + 1) / 2;
		if (num < 4)
		{
			num = 4;
		}
		if (num > 16)
		{
			num = 16;
		}
		return num;
	}

	private void ExpandPool(int count)
	{
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/skillEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbSkillEffect component = val2.GetComponent<zbSkillEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
