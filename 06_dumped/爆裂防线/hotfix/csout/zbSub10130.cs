using System;
using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbSub10130 : zbSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBounds2D bounds2D1;

	private zbBounds2D bounds2D2;

	[NonSerialized]
	public GameObject sizeObject;

	private float m_durationPre;

	private float m_duration;

	protected override void OnInited()
	{
		base.OnInited();
		sizeObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "size");
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_duration = GetAttrValueByType(EAttrSkillType.Duration);
		m_durationPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10021.ogg");
	}

	protected override void Update()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		m_durationPre += Time.deltaTime;
		if (m_durationPre >= m_duration)
		{
			((MonoBehaviour)this).StartCoroutine(ActionAmimation());
			m_durationPre = -999999f;
		}
		if (m_durationPre > 0f && m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			float num = sizeObject.transform.position.x - 1.35f;
			bounds2D1.center.x = num;
			bounds2D1.center.y = 0f;
			bounds2D1.halfSize.x = 1.35f;
			bounds2D1.halfSize.y = 8.4f;
			UpdateMonsterCollision(bounds2D1, ignoreMonsters, EAttrSkillType.Damage);
			float x = 0f - num;
			bounds2D2.center.x = x;
			bounds2D2.center.y = 0f;
			bounds2D2.halfSize.x = 1.35f;
			bounds2D2.halfSize.y = 8.4f;
			UpdateMonsterCollision(bounds2D2, ignoreMonsters, EAttrSkillType.Damage);
		}
	}

	public IEnumerator ActionAmimation()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		ReturnToPool();
	}
}
