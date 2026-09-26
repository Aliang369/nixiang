using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbIntervalAoeSkill : zbSkillBase
{
	protected struct DamageTask(zbMonster t, EAttrSkillType type)
	{
		public zbMonster target = t;

		public EAttrSkillType attrsSkillType = type;
	}

	protected Dictionary<int, DamageTask> m_pendingDamageDict = new Dictionary<int, DamageTask>(2000);

	private Queue<int> m_taskQueue = new Queue<int>(2000);

	private const float LOW_QUOTA = 0.0008f;

	private const float HIGH_QUOTA = 0.0015f;

	private const int MAX_PROCESS_PER_FRAME = 32;

	private const int MAX_DAMAGE_COUNT = 2000;

	protected override void OnInited()
	{
		base.OnInited();
		m_pendingDamageDict.Clear();
		m_taskQueue.Clear();
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_pendingDamageDict.Clear();
		m_taskQueue.Clear();
	}

	public void UpdateMonsterCollisionInBatches(zbBoundsCircle2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType)
	{
		if (base.m_combatMonsterGrid != null)
		{
			base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			AddDamageTaskToDic(_attrSkillType);
		}
	}

	public void UpdateMonsterCollisionInBatches(zbBounds2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType)
	{
		if (base.m_combatMonsterGrid != null)
		{
			base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			AddDamageTaskToDic(_attrSkillType);
		}
	}

	public void UpdateMonsterCollisionInBatches(zbOBB2D boundsCircle2D, HashSet<int> ignoreList, EAttrSkillType _attrSkillType)
	{
		if (base.m_combatMonsterGrid != null)
		{
			base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreList, overlapResultList);
			AddDamageTaskToDic(_attrSkillType);
		}
	}

	private void AddDamageTaskToDic(EAttrSkillType _attrSkillType)
	{
		foreach (zbMonster overlapResult in overlapResultList)
		{
			if (m_pendingDamageDict.Count >= 2000)
			{
				break;
			}
			int uUID = overlapResult.GetUUID();
			if (m_pendingDamageDict.TryAdd(uUID, new DamageTask(overlapResult, _attrSkillType)))
			{
				m_taskQueue.Enqueue(uUID);
			}
		}
	}

	public void ProcessPendingDamageQueue()
	{
		if (m_taskQueue.Count == 0)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = ((m_taskQueue.Count > 500) ? 0.0015f : 0.0008f);
		int num2 = 0;
		while (m_taskQueue.Count > 0 && num2 < 32 && !(Time.realtimeSinceStartup - realtimeSinceStartup > num))
		{
			int key = m_taskQueue.Dequeue();
			if (m_pendingDamageDict.TryGetValue(key, out var value))
			{
				if ((Object)(object)value.target != (Object)null)
				{
					CalcMonsterDamage(value.attrsSkillType, value.target);
				}
				m_pendingDamageDict.Remove(key);
			}
			num2++;
		}
	}
}
