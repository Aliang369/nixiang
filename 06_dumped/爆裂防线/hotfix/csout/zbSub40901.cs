using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40901 : zbSkillBase
{
	[NonSerialized]
	private zbBounds2D bounds2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		float num = Math.Clamp(((Component)this).transform.position.x, combatData.monsterSpawner.GetMinX() + 0.3f, combatData.monsterSpawner.GetMaxX() - 0.3f);
		((Component)this).transform.position = new Vector3(num, -5f, ((Component)this).transform.position.z);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		if (!ignoreAnimation)
		{
			string text = "";
			text = ((skillSubRecord.id == 40901) ? "animation/effect/hero/motuo1/motuo1_SkeletonData.asset" : ((skillSubRecord.id != 40902) ? "animation/effect/hero/motuo101/motuo101_SkeletonData.asset" : "animation/effect/hero/motuo2/motuo2_SkeletonData.asset"));
			zbCombatSpineAssetUtil.RefreshSpine(m_skeletonAnimation, text);
			if (skillSubRecord.id == 40901 || skillSubRecord.id == 40902)
			{
				MarkGpuEffectConvert(text);
			}
			((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		}
		if (skillSubRecord.id == 40901)
		{
			m_skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
		}
		else if (skillSubRecord.id == 40902)
		{
			m_skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
		}
		else
		{
			m_skeletonAnimation.AnimationState.SetAnimation(1, "fire", true);
		}
		ReturnToPool(15f);
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
