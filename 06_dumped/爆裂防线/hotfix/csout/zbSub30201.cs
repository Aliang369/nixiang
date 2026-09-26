using System;
using Proto.Enums;
using UnityEngine;

public class zbSub30201 : zbSkillBase
{
	[NonSerialized]
	public Vector3 targetP;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		targetP = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		conitnueNext();
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	protected void conitnueNext()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		zbMonster randomMonster = combatData.monsters.GetRandomMonster();
		if ((Object)(object)randomMonster != (Object)null)
		{
			targetP = randomMonster.GetCurPosition();
		}
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (zbUtil.Reached(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(targetP)))
		{
			conitnueNext();
			return;
		}
		skillTF.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(targetP), GetAttrValueByType(EAttrSkillType.MoveSpeed) * Time.deltaTime));
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = skillTF.localScale.x / 2f;
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		}
	}

	public zbSub30201()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		targetP = Vector3.zero;
		base._002Ector();
	}
}
