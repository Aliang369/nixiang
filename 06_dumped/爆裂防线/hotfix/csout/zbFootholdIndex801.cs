using System;
using UnityEngine;

public class zbFootholdIndex801 : zbFootholdSkillBase
{
	public stFootholdIndex801 footholdIndex801Data;

	[NonSerialized]
	private zbBounds2D bounds2D;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Projectile;
		base.OnInited();
		canOutWindow = true;
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex801Data = (stFootholdIndex801)_footholdIndex;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "normal", true);
		bounds2D.center.x = skillTF.position.x;
		bounds2D.center.y = skillTF.position.y;
		bounds2D.halfSize.x = 3.6f;
		bounds2D.halfSize.y = 1.5f;
	}

	public void TriggerDamage()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "out", false, 0f);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "normal", true, 0f);
		base.m_combatMonsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			if (overlapResult.GetMass() <= footholdIndex801Data.massLimit)
			{
				overlapResult.monsterTF.position = new Vector3(combatData.nodeRootPositonX, overlapResult.monsterTF.position.y, overlapResult.monsterTF.position.z);
			}
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, footholdIndex801Data.damagePercent * (float)(long)overlapResult.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
		}
	}
}
