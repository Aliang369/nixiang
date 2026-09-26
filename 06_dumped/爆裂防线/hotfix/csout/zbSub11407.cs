using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub11407 : zbSkillBase
{
	private SkeletonAnimation skeletonDiyuzhimen;

	private bool canFight;

	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	[NonSerialized]
	public Bone gunpoint;

	protected override void OnInited()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		skeletonDiyuzhimen = ((Component)((Component)this).transform.Find("diyuzhimen")).GetComponent<SkeletonAnimation>();
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		((MonoBehaviour)this).StartCoroutine(ActionAmimation());
	}

	public IEnumerator ActionAmimation()
	{
		canFight = true;
		TrackEntry val = skeletonDiyuzhimen.AnimationState.SetAnimation(0, "enter", false);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		skeletonDiyuzhimen.AnimationState.SetAnimation(0, "loop", true);
		yield return (object)new WaitForSeconds(4.5f);
		skillAttrList[EAttrSkillType.Damage] = skillAttrList[EAttrSkillType.Damage] * skillSubRecord.param[0];
		val = skeletonDiyuzhimen.AnimationState.SetAnimation(0, "exit", false);
		canFight = false;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		ReturnToPool();
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
			}
		}
	}
}
