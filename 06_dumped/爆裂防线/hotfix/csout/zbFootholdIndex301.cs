using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbFootholdIndex301 : zbFootholdSkillBase
{
	public stFootholdIndex301 footholdIndex301Data;

	private bool canFight = true;

	protected override void OnInited()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ignoreMonsters.Clear();
		canFight = true;
		if (!ignoreAnimation)
		{
			int num = Rand.Range(1, 6);
			string spinePath = "animation/foothold/scene_point_candyland_" + num + "/scene_point_candyland_" + num + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_skeletonAnimation, spinePath);
			((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		}
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex301Data = (stFootholdIndex301)_footholdIndex;
		skillAttrList[EAttrSkillType.SlowDown] = footholdIndex301Data.slowDownPrecent;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
		((Component)this).transform.localScale = Vector3.one * footholdIndex301Data.damageRange;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (canFight)
		{
			zbMonster component = ((Component)collider).GetComponent<zbMonster>();
			if ((Object)(object)component != (Object)null)
			{
				TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
				ShootFormulaMonster(component.GetUUID());
				ReturnToPool(val.AnimationEnd);
				canFight = false;
			}
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null && monsterById.GetMass() <= footholdIndex301Data.massLimit)
		{
			stDamageResult stDamageResult2 = zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, footholdIndex301Data.damagePercent * (float)(long)monsterById.maxHp, footholdEffectRecord.atkType, ignoreAttackTypeAdd: true);
			if (stDamageResult2.damageType == EDamageResult.Hit && stDamageResult2.damageValue > 0)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 1, footholdIndex301Data.slowDownTime);
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 29, footholdIndex301Data.slowDownTime, 1, footholdIndex301Data.scalePercent);
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 30, footholdIndex301Data.slowDownTime, 1, footholdIndex301Data.massPercent);
			}
		}
	}
}
