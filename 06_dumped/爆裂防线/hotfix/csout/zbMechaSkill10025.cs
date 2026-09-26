using UnityEngine;

public class zbMechaSkill10025 : zbMechaSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		base.OnInited();
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[1] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = num / 2f;
		UpdateMonsterCollision();
		ReturnToPool(1.6666666f);
	}

	private void UpdateMonsterCollision()
	{
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, base.damage, mechaSkillRecord.atkType);
		}
	}
}
