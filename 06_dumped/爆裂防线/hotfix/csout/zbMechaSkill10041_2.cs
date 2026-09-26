using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10041_2 : zbMechaSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 16f;

	[NonSerialized]
	private static float boxRadius = 0.06f;

	[NonSerialized]
	private zbBounds2D bounds2D;

	private int penetration;

	protected override void OnInited()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		((Component)this).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
		ignoreMonsters.Clear();
		ReturnToPool(lifetime);
		currPenetration = 0;
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)0;
		}
		else
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)1;
		}
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		bulletSpeed = mechaSkillRecord.param[7] / 100f;
		penetration = (int)mechaSkillRecord.param[6];
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)this).transform.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		Vector3 position = ((Component)this).transform.position;
		Vector3 localScale = ((Component)this).transform.localScale;
		bounds2D.center.x = position.x;
		bounds2D.center.y = position.y;
		bounds2D.halfSize.x = boxRadius * localScale.x;
		bounds2D.halfSize.y = boxRadius * localScale.y;
		combatData.monsters.monsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
		using (List<zbMonster>.Enumerator enumerator = overlapResultList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				zbMonster current = enumerator.Current;
				ignoreMonsters.Add(current.GetUUID());
				TakeDamage(current);
			}
		}
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
	}

	private void TakeDamage(zbMonster targetMonster)
	{
		if (currPenetration < penetration + 1)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, base.damage, mechaSkillRecord.atkType);
		}
		currPenetration++;
		if (currPenetration >= penetration + 1)
		{
			ReturnToPool();
		}
	}
}
