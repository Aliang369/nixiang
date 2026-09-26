using Proto.Enums;
using UnityEngine;

public class zbSub10705 : zbSkillBase
{
	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		}
	}
}
