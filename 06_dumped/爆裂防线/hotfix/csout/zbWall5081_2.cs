using System;
using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbWall5081_2 : zbWallSkillBase
{
	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	public stWallSkillIndex5081 wallSkillIndex5081Data;

	private float m_durationPre;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.BurnLayer;
		base.OnInited();
		zbAudioMgr.Instance.PlayAudio("audio/wall/hailang.ogg");
		m_skeletonAnimation.AnimationState.SetAnimation(0, "open", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		base.SetWallAtr(_damage, _wallSkillRecord);
		m_damageIntervalPre = wallSkillIndex5081Data.damageInterval;
		m_damageInterval = wallSkillIndex5081Data.damageInterval;
		m_durationPre = 0f;
		((Component)this).transform.localScale = new Vector3(wallSkillIndex5081Data.damageRange, wallSkillIndex5081Data.damageRange, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = wallSkillIndex5081Data.damageRange / 2f;
		skillAttrList[EAttrSkillType.SlowDown] = wallSkillIndex5081Data.slowDumpPrecent2;
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
			foreach (zbMonster overlapResult in overlapResultList)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, overlapResult.GetUUID(), 1, wallSkillIndex5081Data.slowDumpTime2);
			}
		}
		m_durationPre += Time.deltaTime;
		if (m_durationPre >= wallSkillIndex5081Data.duration)
		{
			m_durationPre = 0f;
			((MonoBehaviour)this).StartCoroutine(Chuansong());
		}
	}

	public IEnumerator Chuansong()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "close", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, combatData.defense.GetAlertPositionY() + wallSkillIndex5081Data.defenseY, ((Component)this).transform.position.z);
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			if (!overlapResult.isEliteBoss)
			{
				((Component)overlapResult).transform.position = new Vector3(((Component)overlapResult).transform.position.x, combatData.defense.GetAlertPositionY() + wallSkillIndex5081Data.defenseY, ((Component)overlapResult).transform.position.z);
			}
		}
		val = m_skeletonAnimation.AnimationState.SetAnimation(0, "open", false);
		ReturnToPool(val.AnimationEnd);
	}
}
