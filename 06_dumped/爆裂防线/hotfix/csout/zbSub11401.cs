using Proto.Enums;
using UnityEngine;

public class zbSub11401 : zbSkillBase
{
	private Vector3 targetP;

	private float bulletSpeed = 1f;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		targetP = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		bulletSpeed = GetAttrValueByType(EAttrSkillType.MoveSpeed);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		conitnueNext();
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
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
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (zbUtil.Reached(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(targetP)))
		{
			conitnueNext();
			return;
		}
		((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(targetP), bulletSpeed * Time.deltaTime));
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		}
	}
}
