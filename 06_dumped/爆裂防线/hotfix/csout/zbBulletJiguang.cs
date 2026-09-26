using Proto.Enums;
using UnityEngine;

public class zbBulletJiguang : zbSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		base.OnInited();
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		triggerEndMod = false;
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			DetectLaserCollision();
		}
	}

	private void DetectLaserCollision()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
	}
}
