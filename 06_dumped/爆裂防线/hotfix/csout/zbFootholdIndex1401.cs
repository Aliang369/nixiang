using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbFootholdIndex1401 : zbFootholdSkillBase
{
	public stFootholdIndex1401 footholdIndex1401Data;

	private bool canFight;

	private float m_damageDurationPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		ignoreMonsters.Clear();
		m_skeletonAnimation.AnimationState.SetAnimation(0, "normal", true);
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex1401Data = (stFootholdIndex1401)_footholdIndex;
		canFight = false;
		skillAttrList[EAttrSkillType.SlowDown] = 1f;
	}

	public void TriggerDamage()
	{
		m_skeletonAnimation.AnimationState.SetAnimation(0, "act", true);
		UpdateMonsterCollision();
		m_damageDurationPre = 0f;
		canFight = true;
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_damageDurationPre += Time.deltaTime;
			if (m_damageDurationPre >= footholdIndex1401Data.durationTime)
			{
				canFight = false;
				m_skeletonAnimation.AnimationState.SetAnimation(0, "normal", true);
			}
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			ShootFormulaMonster(monster.Value);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 1701, footholdIndex1401Data.durationTime);
	}
}
