using System;
using Proto.Enums;
using UnityEngine;

public class zbBulletMotuo : zbSkillBase
{
	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private GameObject shuiwenGO;

	protected override bool IsGpuBulletExcluded
	{
		get
		{
			if ((Object)(object)hero != (Object)null && hero.team != null)
			{
				return hero.team.modMgr.HasModIndex(2092);
			}
			return false;
		}
	}

	protected override void OnInited()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		shuiwenGO = ((Component)((Component)this).transform.Find("shuiwen")).gameObject;
		shuiwenGO.SetActive(false);
		shuiwenGO.SetActive(zbCombatSystem.Instance.bMonsterNeedWaterEffect);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		float num = Math.Clamp(((Component)this).transform.position.x, combatData.monsterSpawner.GetMinX() + 0.3f, combatData.monsterSpawner.GetMaxX() - 0.3f);
		((Component)this).transform.position = new Vector3(num, -5f, ((Component)this).transform.position.z);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		ReturnToPool(15f);
		if (hero.team.modMgr.HasModIndex(2092))
		{
			m_skeletonAnimation.AnimationState.SetAnimation(1, "fire", true);
		}
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			bounds2D.center.x = skillTF.position.x;
			bounds2D.center.y = skillTF.position.y;
			bounds2D.halfSize.x = 0.5f * skillTF.localScale.x;
			bounds2D.halfSize.y = 0.9f * skillTF.localScale.y;
			UpdateMonsterCollision(bounds2D, ignoreMonsters, EAttrSkillType.Damage);
		}
	}
}
