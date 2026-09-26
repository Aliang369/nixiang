using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMechaSkill10045_2 : zbMechaSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float duration;

	private bool canFight;

	protected override void OnInited()
	{
		base.OnInited();
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		canFight = false;
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[0] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		duration = mechaSkillRecord.param[2];
		m_damageIntervalPre = mechaSkillRecord.param[1];
		m_damageInterval = m_damageIntervalPre;
	}

	public IEnumerator ShootRepeater()
	{
		TrackEntry current = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(1.1f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/104_hedan.ogg");
		canFight = true;
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.1f);
		}
		else
		{
			yield return (object)new WaitForSeconds(3.3333333f);
		}
		ReturnToPool();
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			needDurationPre += Time.deltaTime;
			if (needDurationPre >= duration)
			{
				needDurationPre = 0f;
				ReturnToPool();
			}
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision();
			}
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monster.Value, base.damage, mechaSkillRecord.atkType);
		}
	}
}
