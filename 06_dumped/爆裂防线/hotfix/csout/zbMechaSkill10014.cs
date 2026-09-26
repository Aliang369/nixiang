using System;
using System.Collections.Generic;
using UnityEngine;

public class zbMechaSkill10014 : zbMechaSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	[NonSerialized]
	public float bulletSpeed = 16f;

	private AudioSource m_audio;

	protected override void OnInited()
	{
		base.OnInited();
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/103_3.ogg", loop: true);
		ignoreMonsters.Clear();
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		bulletSpeed = mechaSkillRecord.param[2] / 100f;
		m_damageIntervalPre = 0f;
		m_damageInterval = mechaSkillRecord.param[1];
		ReturnToPool(99f);
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

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
		StopLoopAudio();
	}

	private void StopLoopAudio()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)this).transform.Translate(Vector3.up * bulletSpeed * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
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
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
		}
	}
}
