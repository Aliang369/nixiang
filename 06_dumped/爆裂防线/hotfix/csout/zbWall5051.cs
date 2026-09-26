using System.Collections.Generic;
using UnityEngine;

public class zbWall5051 : zbWallSkillBase
{
	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	private zbBoundsCircle2D boundsCircle2D;

	public stWallSkillIndex5051 wallSkillIndex5051Data;

	protected override void OnInited()
	{
		base.OnInited();
		m_monstersInRange.Clear();
		ReturnToPool(0.7f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.SetWallAtr(_damage, _wallSkillRecord);
		float num = wallSkillIndex5051Data.damageRadius;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = num / 2f;
		UpdateMonsterCollision();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, m_monstersInRange);
		foreach (zbMonster item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		if ((Object)(object)targetMonster != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, base.damage, wallSkillRecord.atkType);
		}
	}
}
