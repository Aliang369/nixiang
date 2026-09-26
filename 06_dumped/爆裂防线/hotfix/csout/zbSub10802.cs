using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10802 : zbSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 1f;

	private zbOBB2D boundsOBB2D;

	private int Penetration;

	protected override void OnInited()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
		Penetration = (int)GetAttrValueByType(EAttrSkillType.Penetration) + 1;
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		((Component)this).transform.Translate(m_direction * 0.5f, (Space)0);
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		ignoreMonsters.Clear();
		ignoreMonsters.Add(mainMonsterUUID);
		ReturnToPool(lifetime);
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
		base.Update();
		skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		boundsOBB2D.SetPram(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(m_direction), 1f * skillTF.localScale.x, 0.2f * skillTF.localScale.x);
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryFirst(in boundsOBB2D, ignoreMonsters, overlapResultList);
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
		if (currPenetration < Penetration)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monster);
		}
		currPenetration++;
		if (currPenetration >= Penetration)
		{
			ReturnToPool();
		}
	}
}
