using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10701 : zbSkillBase
{
	private float bulletSpeed = 1f;

	[NonSerialized]
	private static float boxRadius = 0.3f;

	[NonSerialized]
	private zbBounds2D bounds2D;

	protected override void OnInited()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			Vector2 direction = Vector2.op_Implicit(zbUtil.GetTargetNormalized(((Component)this).transform.position, monsterById.GetCurPosition()));
			SetDirection(direction);
		}
		else
		{
			Vector2 val = new Vector2(Rand.Range(-1f, 1f), 1f);
			Vector2 normalized = ((Vector2)(ref val)).normalized;
			SetDirection(normalized);
		}
		currPenetration = 0;
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		((Component)this).transform.Translate(m_direction * 0.5f, (Space)0);
		ignoreMonsters.Clear();
		ignoreMonsters.Add(mainMonsterUUID);
		ReturnToPool(lifetime);
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = boxRadius * skillTF.localScale.x;
		bounds2D.halfSize.y = boxRadius * skillTF.localScale.y;
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
		using List<zbMonster>.Enumerator enumerator = overlapResultList.GetEnumerator();
		if (enumerator.MoveNext())
		{
			zbMonster current = enumerator.Current;
			ignoreMonsters.Add(current.GetUUID());
			TakeDamage(current);
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
