using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub41401 : zbSkillBase
{
	private zbOBB2D boundsOBB2D;

	private float bulletSpeed;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnInited()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		currPenetration = 0;
		ignoreMonsters.Clear();
		ReturnToPool(lifetime);
		if (skillSubRecord.id == 41401)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.magenta);
		}
		else if (skillSubRecord.id == 41402)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.green);
		}
		else if (skillSubRecord.id == 41403)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_skeletonAnimation).skeleton, Color.blue);
		}
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)0;
		}
		else
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)1;
		}
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		boundsOBB2D.SetPram(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(m_direction), 1f * skillTF.localScale.x, 0.2f * skillTF.localScale.x);
		UpdateMonsterCollision();
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryFirst(in boundsOBB2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ignoreMonsters.Add(overlapResult.GetUUID());
			TakeDamage(overlapResult);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		if ((float)currPenetration < GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monster);
		}
		currPenetration++;
		if ((float)currPenetration >= GetAttrValueByType(EAttrSkillType.Penetration) + 1f)
		{
			ReturnToPool();
		}
	}
}
