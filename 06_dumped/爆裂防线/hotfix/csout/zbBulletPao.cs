using System;
using Proto.Enums;
using UnityEngine;

public class zbBulletPao : zbSkillBase
{
	[NonSerialized]
	private zbBounds2D bounds2D;

	[NonSerialized]
	private static float boxRadius = 0.16f;

	private int penetration;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		penetration = (int)GetAttrValueByType(EAttrSkillType.Penetration);
		triggerEndMod = false;
		currPenetration = 0;
		ReturnToPool(5f);
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * GetAttrValueByType(EAttrSkillType.BulletSpeed) * Time.deltaTime, (Space)0);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = boxRadius * skillTF.localScale.x;
		bounds2D.halfSize.y = boxRadius * skillTF.localScale.y;
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsters.monsterGrid.QueryFirst(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			if (bounds2D.Overlaps(overlapResult.monsterBoxBounds))
			{
				ignoreMonsters.Add(overlapResult.GetUUID());
				TakeDamage(overlapResult);
				break;
			}
		}
	}

	private void TakeDamage(zbMonster monster)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (currPenetration < penetration + 1 && CalcMonsterDamage(EAttrSkillType.Damage, monster).damageValue > 0)
		{
			zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/pao/bulletPaoBoom.prefab", hero, ((Component)this).transform.position, Vector2.up, monster.GetUUID());
			foreach (stModIndex2022 item in hero.team.modIndex2022)
			{
				if (item.skillType == hero.GetSkillType() && zbUtil.Roll0to1(item.roll0to1))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monster.GetUUID(), item.subBuffId, item.extraTime);
				}
			}
		}
		currPenetration++;
		if (currPenetration >= penetration + 1)
		{
			ReturnToPool();
		}
	}
}
