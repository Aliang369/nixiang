using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub10211 : zbSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private bool canFire;

	private zbBoundsCircle2D boundsCircle2D;

	public SkeletonAnimation huoqiuAnimation;

	[NonSerialized]
	public Bone boneShoot;

	protected override void OnInited()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		skillAttrList[EAttrSkillType.BurnDamage] = (float)(int)hero.team.teamAttack * skillSubRecord.atkValRate;
		canFire = false;
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration) + 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		boneShoot = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shoot");
		zbMonster randomMonster = combatData.monsters.GetRandomMonster();
		if ((Object)(object)randomMonster != (Object)null)
		{
			Vector3 val = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(randomMonster.GetCurPosition());
			SkeletonExtensions.SetLocalPosition(boneShoot, val);
		}
		else
		{
			float num = Rand.Range(-3f, 3f);
			float num2 = Rand.Range(-5f, 6f);
			Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(new Vector2(num, num2) + new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)));
			SkeletonExtensions.SetLocalPosition(boneShoot, val2);
		}
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)huoqiuAnimation).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		((Component)huoqiuAnimation).gameObject.SetActive(false);
		((MonoBehaviour)this).StartCoroutine(SpawnInit());
	}

	private IEnumerator SpawnInit()
	{
		m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(5f / 6f);
		((Component)huoqiuAnimation).gameObject.SetActive(true);
		TrackEntry val = huoqiuAnimation.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		huoqiuAnimation.AnimationState.SetAnimation(0, "loop", true);
		canFire = true;
	}

	protected override void Update()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (canFire)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				boundsCircle2D.center.x = ((Component)huoqiuAnimation).transform.position.x;
				boundsCircle2D.center.y = ((Component)huoqiuAnimation).transform.position.y;
				boundsCircle2D.radius = ((Component)huoqiuAnimation).transform.localScale.x / 2f;
				UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
			}
		}
	}
}
