using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbBulletBingbao : zbSkillBase
{
	private bool isFirst;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	protected override void OnInited()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		m_DurationMax = GetAttrValueByType(EAttrSkillType.Duration);
		m_DurationTime = 0f;
		m_bExecReturn = false;
		PlayBulletAnim("1", loop: false);
		QueueBulletAnim("2", loop: true);
		m_skeletonAnimation.UpdateTiming = (UpdateTiming)(!zbCombatSystem.Instance.combatFPS.IsVeryLowPerfomanceMode());
		CentralDamageMonster(mainMonsterUUID);
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
		float deltaTime = Time.deltaTime;
		m_damageIntervalPre += deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		m_DurationTime += deltaTime;
		if (!m_bExecReturn && m_DurationTime >= m_DurationMax)
		{
			m_DurationTime = 0f;
			m_bExecReturn = true;
			ActionRoutine();
		}
	}

	private void UpdateMonsterCollision()
	{
		base.m_combatMonsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			CalcMonsterDamage(EAttrSkillType.PeripheralDamage, overlapResult);
		}
		if (isFirst)
		{
			if (overlapResultList.Count == 1 && hero.team.ExtraProperties[ETeamExtra.GemMod2101] > 0f)
			{
				hero.nextFireTime += GetAttrValueByType(EAttrSkillType.Cd) * hero.team.ExtraProperties[ETeamExtra.GemMod2101];
			}
			isFirst = false;
		}
	}

	private void ActionRoutine()
	{
		m_skeletonAnimation.AnimationState.ClearTracks();
		TrackEntry val = PlayBulletAnim("3", loop: false);
		if (val != null)
		{
			ReturnToPool(val.AnimationEnd);
		}
		else
		{
			ReturnToPool();
		}
	}

	private void CentralDamageMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.CentralDamage, monsterById);
		}
	}

	public override void SetFirst(bool _isFirst)
	{
		isFirst = _isFirst;
	}
}
