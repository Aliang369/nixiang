using System;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterBullet : MonoBehaviour
{
	public zbCombatData combatData;

	public zbMonster monster;

	private int monsterTid;

	public zbMonsterCsvRecord monsterRecord;

	public zbSpineGpuAnimPlayer gpuPlayer;

	public bool m_spineGpuTakenOver;

	public SkeletonAnimation animationSpine;

	private float speed = 6f;

	private float fixedDamage;

	private float m_DurationMax;

	private float m_DurationTime;

	private int m_bExecReturn;

	private float m_attackBulletHit;

	private float m_attackBulletHitTime;

	private float hitRatePercent;

	private void Reset()
	{
	}

	public void Init(zbCombatData _combatData, zbMonster _monster, float _fixedDamage = 0f)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		zbSpineGpuAnimRuntime.RestoreDormantBullet(this);
		m_attackBulletHitTime = 0f;
		m_attackBulletHit = 0f;
		m_bExecReturn = 0;
		m_DurationTime = 0f;
		fixedDamage = _fixedDamage;
		combatData = _combatData;
		monster = _monster;
		monsterTid = monster.monsterRecord.id;
		monsterRecord = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		((Component)this).transform.position = ((Component)monster).transform.position;
		((Component)this).gameObject.transform.SetParent(((Component)monster).transform.parent);
		speed = 6f;
		m_DurationMax = Math.Abs(((Component)this).transform.position.y + 6f) / speed;
		GameObject val = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimation");
		if ((Object)(object)val != (Object)null)
		{
			animationSpine = val.GetComponent<SkeletonAnimation>();
			if (((SkeletonRenderer)animationSpine).Skeleton.Data.FindAnimation("attack_bullet") != null)
			{
				animationSpine.AnimationState.SetAnimation(0, "attack_bullet", true);
			}
		}
		if (fixedDamage <= 0f)
		{
			fixedDamage = monster.GetAttrValueByType(EAttrMonsterType.Damage);
		}
		hitRatePercent = monster.GetHitRatePercent();
		RefreshRiseEffect();
		if (zbSpineGpuAnimRuntime.TryConvertBullet(this))
		{
			gpuPlayer.PlayClip("attack_bullet", loop: true);
			gpuPlayer.Advance(0f);
		}
	}

	public void RefreshRiseEffect()
	{
		if ((Object)(object)animationSpine != (Object)null)
		{
			string spinePath = monster.monsterAvatarRecord.fightBone + "_SkeletonData.asset";
			zbCombatSpineAssetUtil.RefreshSpine(animationSpine, spinePath);
			if (((SkeletonRenderer)animationSpine).Skeleton != null && ((SkeletonRenderer)animationSpine).Skeleton.Data.FindAnimation("attack_bullet") != null)
			{
				animationSpine.AnimationState.SetAnimation(0, "attack_bullet", true);
			}
		}
	}

	protected void Update()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (m_spineGpuTakenOver && gpuPlayer != null)
		{
			gpuPlayer.Advance(Time.deltaTime);
		}
		if (m_bExecReturn == 0)
		{
			((Component)this).transform.Translate(Vector3.down * speed * Time.deltaTime, (Space)0);
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = 1;
				if (((SkeletonRenderer)animationSpine).Skeleton.Data.FindAnimation("attack_bullet_hit") != null)
				{
					TrackEntry val = animationSpine.AnimationState.SetAnimation(0, "attack_bullet_hit", false);
					m_attackBulletHit = val.AnimationEnd;
				}
			}
		}
		if (m_bExecReturn == 1)
		{
			m_attackBulletHitTime += Time.deltaTime;
			if (m_attackBulletHitTime >= m_attackBulletHit)
			{
				m_bExecReturn = 2;
				EnableColliderAfterDelay();
			}
		}
	}

	public void EnableColliderAfterDelay()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		speed = 0f;
		zbCombatSystem.Instance.combatFormula.CalcWallDamage(null, combatData.defense, fixedDamage, ((Component)this).transform.position.x, hitRatePercent);
		combatData.monsterBulletEffectPoolMgr.ReturnMonsterBulletEffect(this);
	}

	private void OnDisable()
	{
		zbSpineGpuAnimRuntime.ReleaseBullet(this);
	}
}
