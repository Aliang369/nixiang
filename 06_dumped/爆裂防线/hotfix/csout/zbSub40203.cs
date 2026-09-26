using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40203 : zbSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 1f;

	[NonSerialized]
	private static float boxRadius = 0.16f;

	[NonSerialized]
	private zbBounds2D bounds2D;

	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private SkeletonAnimation spineBullet;

	private SkeletonAnimation spineFire;

	private SkeletonAnimation spineBoom;

	private int currState = 1;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		((Component)this).transform.Translate(m_direction * 0.5f, (Space)0);
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
		ignoreMonsters.Clear();
		ignoreMonsters.Add(mainMonsterUUID);
		spineBullet = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimation").GetComponent<SkeletonAnimation>();
		spineBoom = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimationBoom").GetComponent<SkeletonAnimation>();
		spineFire = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimationFire").GetComponent<SkeletonAnimation>();
		((Component)spineBoom).gameObject.SetActive(false);
		((Component)spineFire).gameObject.SetActive(false);
		((Component)spineBullet).gameObject.SetActive(true);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		currState = 1;
		ReturnToPool(10f);
	}

	protected override void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = boxRadius * skillTF.localScale.x;
		bounds2D.halfSize.y = boxRadius * skillTF.localScale.y;
		if (currState == 1)
		{
			skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
			combatData.monsters.monsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
			using List<zbMonster>.Enumerator enumerator = overlapResultList.GetEnumerator();
			if (enumerator.MoveNext())
			{
				zbMonster current = enumerator.Current;
				((Component)spineBullet).gameObject.SetActive(false);
				currState = 2;
				CalcMonsterDamage(EAttrSkillType.Damage, current);
				((Component)this).transform.up = Vector3.up;
				float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
				((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
				((Component)spineBoom).gameObject.SetActive(true);
				spineBoom.AnimationState.SetAnimation(0, "animation", false);
				((Component)spineFire).gameObject.SetActive(true);
				ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
				boundsCircle2D.center.x = skillTF.position.x;
				boundsCircle2D.center.y = skillTF.position.y;
				boundsCircle2D.radius = attrValueByType / 2f;
			}
		}
		if (currState == 2)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision();
			}
		}
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			CalcMonsterDamage(EAttrSkillType.ExplosionDamage, overlapResult);
		}
	}
}
