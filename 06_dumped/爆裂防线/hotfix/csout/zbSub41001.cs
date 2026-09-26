using System;
using Proto.Enums;
using UnityEngine;

public class zbSub41001 : zbSkillBase
{
	[NonSerialized]
	private zbBoundsCircle2D boundsCircle2D;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		CentralDamageMonster(mainMonsterUUID);
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.PeripheralDamage);
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

	protected override void OnDisable()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisable();
		if (skillSubRecord.id == 41003)
		{
			zbSkillSystem.Instance.CreateSubSkill((int)skillSubRecord.param[0], hero, null, skillAttrList, ((Component)this).transform.position, Vector2.up);
		}
	}
}
