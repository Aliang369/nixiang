using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbWall5031 : zbWallSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private bool isEnter;

	private bool canFight;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_damageDuration;

	private float m_LeftCd;

	[NonSerialized]
	private Bone boneShoot1;

	[NonSerialized]
	private GameObject goRiseEffect_1;

	[NonSerialized]
	private GameObject shootPoint_1;

	public stWallSkillIndex5031 wallSkillIndex5031Data;

	private AudioSource m_audio;

	protected override void OnInited()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		ignoreAnimation = true;
		isEnter = false;
		base.OnInited();
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		ignoreMonsters.Clear();
		m_skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
		canFight = false;
		shootPoint_1 = ((Component)((Component)this).transform.Find("leftShoot")).gameObject;
		boneShoot1 = ((SkeletonRenderer)m_skeletonAnimation).skeleton.FindBone("shoot1");
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("leftObject")).gameObject;
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localScale = Vector3.one + 2f * Vector3.down;
		}
		else
		{
			((Component)this).transform.localScale = Vector3.one;
		}
		ReturnToPool(99999f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		base.SetWallAtr(_damage, _wallSkillRecord);
		m_damageIntervalPre = wallSkillIndex5031Data.damageInterval;
		m_damageInterval = wallSkillIndex5031Data.damageInterval;
		m_LeftCd = wallSkillIndex5031Data.cd;
	}

	private void StopLoopAudio()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(val));
			SkeletonExtensions.SetLocalPosition(boneShoot1, val2);
			Vector3 targetNormalized = zbUtil.GetTargetNormalized(goRiseEffect_1.transform.position, Vector2.op_Implicit(val));
			goRiseEffect_1.transform.up = targetNormalized;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (isEnter)
		{
			if (canFight)
			{
				m_damageIntervalPre += Time.deltaTime;
				m_damageDuration += Time.deltaTime;
				if (m_damageIntervalPre >= m_damageInterval)
				{
					m_damageIntervalPre -= m_damageInterval;
					UpdateMonsterCollision();
				}
				if (m_damageDuration >= (float)wallSkillIndex5031Data.duration)
				{
					m_damageDuration = 0f;
					((MonoBehaviour)this).StartCoroutine(ActionExit());
				}
			}
		}
		else if (!canFight)
		{
			m_LeftCd += Time.deltaTime;
			if (m_LeftCd >= (float)wallSkillIndex5031Data.cd)
			{
				((MonoBehaviour)this).StartCoroutine(ActionEnter());
			}
		}
	}

	public IEnumerator ActionEnter()
	{
		StopLoopAudio();
		isEnter = true;
		zbMonster closestMonster;
		while (true)
		{
			closestMonster = combatData.monsters.GetClosestMonster(wallSkillIndex5031Data.alertRange);
			if (!((Object)(object)closestMonster == (Object)null))
			{
				break;
			}
			yield return null;
		}
		targetMonsterUUID = closestMonster.GetUUID();
		UpdateLaser();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/gaonengshexian.ogg", loop: true);
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "attack_enter", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", true);
		canFight = true;
		m_LeftCd = 0f;
		wallSkillIndex5031Data.DesShootNumber();
	}

	public IEnumerator ActionExit()
	{
		StopLoopAudio();
		canFight = false;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "attack_exit", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
		m_damageDuration = 0f;
		isEnter = false;
		if ((int)wallSkillIndex5031Data.shootNumber <= 0)
		{
			ReturnToPool();
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}

	private void UpdateMonsterCollision()
	{
		if (!canFight)
		{
			return;
		}
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, wallSkillRecord.atkType);
		}
	}
}
