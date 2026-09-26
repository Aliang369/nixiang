using System;
using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbSub10210 : zbSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private bool canFire;

	[NonSerialized]
	public GameObject actionObject;

	protected override void OnInited()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Transform obj = ((Component)this).transform.Find("goRiseEffect_1");
		actionObject = ((obj != null) ? ((Component)obj).gameObject : null);
		base.OnInited();
		canFire = false;
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration) + 2f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		((MonoBehaviour)this).StartCoroutine(SpawnInit());
	}

	private IEnumerator SpawnInit()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		canFire = true;
	}

	protected override void Update()
	{
		base.Update();
		if (canFire)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				SpawnAction();
			}
		}
	}

	private void SpawnAction()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)skillSubRecord.param[0];
		int subBulletId = (int)skillSubRecord.param[1];
		for (int i = 0; i < num; i++)
		{
			zbSkillSystem.Instance.CreateSubSkill(subBulletId, hero, this, skillAttrList, actionObject.transform.position, Vector2.up);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "action", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10042_2.ogg");
	}
}
