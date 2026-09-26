using Proto.Enums;
using Spine;
using UnityEngine;

public class zbBulletDianci : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	private float hitTime;

	protected override void OnInited()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		hitTime = 0f;
		ReturnToPool(val.AnimationEnd);
	}

	protected override void Update()
	{
		base.Update();
		hitTime += Time.deltaTime;
		if (hitTime >= 1f / 3f)
		{
			UpdateMonsterCollision();
			hitTime = -9999f;
		}
	}

	private void UpdateMonsterCollision()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			TakeDamage(overlapResult);
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		CalcMonsterDamage(EAttrSkillType.SplashDamage, monster);
		for (int i = 0; (float)i < hero.team.ExtraProperties[ETeamExtra.DianciMod2071]; i++)
		{
			CalcMonsterDamage(EAttrSkillType.SplashDamage, monster);
		}
	}
}
