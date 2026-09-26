using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub11303 : zbSkillBase
{
	private float bulletSpeed = 1f;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			Vector2 direction = Vector2.op_Implicit(zbUtil.GetTargetNormalized(((Component)this).transform.position, monsterById.GetCurPosition()));
			SetDirection(direction);
		}
		else
		{
			Vector2 val = new Vector2(Rand.Range(-1f, 1f), Rand.Range(-1f, 1f));
			Vector2 normalized = ((Vector2)(ref val)).normalized;
			SetDirection(normalized);
		}
		((Component)this).transform.Translate(m_direction * 0.5f, (Space)0);
		currPenetration = 0;
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		ignoreMonsters.Clear();
		ignoreMonsters.Add(mainMonsterUUID);
		((Component)this).transform.up = Vector3.up;
		ReturnToPool(lifetime);
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = 0.15f;
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryFirst(in boundsCircle2D, ignoreMonsters, overlapResultList);
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
