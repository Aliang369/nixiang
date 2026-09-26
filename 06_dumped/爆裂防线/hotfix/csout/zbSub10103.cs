using System;
using Proto.Enums;
using UnityEngine;

public class zbSub10103 : zbSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 1f;

	[NonSerialized]
	private static float boxRadius = 0.06f;

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
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
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
		((Component)this).transform.localScale = new Vector3(1.25f, 1.25f, 1f);
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
		UpdateMonsterCollision(bounds2D, ignoreMonsters, EAttrSkillType.Damage, stopAtFirst: true);
	}
}
