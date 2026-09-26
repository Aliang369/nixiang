using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40503 : zbSkillBase
{
	private SkeletonAnimation zhadanAnimation;

	private float speed = 8f;

	private float damageRange;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbBoundsCircle2D boundsCircle2D;

	private bool canFightMonster;

	private float m_DurationMax;

	private float m_DurationTime;

	protected override void OnInited()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		canFightMonster = false;
		speed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		damageRange = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(damageRange, damageRange, 1f);
		zhadanAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		zhadanAnimation.AnimationState.SetAnimation(0, "1", true);
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			SetTargetPosition(monsterById.GetCurPosition());
		}
		else
		{
			monsterById = combatData.monsters.GetClosestMonster(100f);
			if ((Object)(object)monsterById != (Object)null)
			{
				SetTargetPosition(monsterById.GetCurPosition());
			}
			else
			{
				SetTargetPosition(new Vector3(0f, 0f, 0f));
			}
		}
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageIntervalPre = m_damageInterval;
	}

	public void SetTargetPosition(Vector3 _targetPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_direction = zbUtil.GetTargetNormalized(((Component)this).transform.position, _targetPosition);
		m_DurationMax = zbUtil.GetVector2Distance(((Component)this).transform.position, _targetPosition) / speed;
		lifetime = GetAttrValueByType(EAttrSkillType.Duration) + m_DurationMax;
		ReturnToPool(lifetime);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.ExplosionDamage);
	}

	protected override void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (speed > 0f)
		{
			skillTF.Translate(m_direction * speed * Time.deltaTime, (Space)0);
		}
		m_DurationTime += Time.deltaTime;
		if (m_DurationTime < m_DurationMax)
		{
			float num = m_DurationTime * (m_DurationMax - m_DurationTime) * 9f + 1f;
			skillTF.localScale = new Vector3(num, num, 1f);
		}
		else if (!canFightMonster)
		{
			skillTF.localScale = new Vector3(damageRange, damageRange, 1f);
			canFightMonster = true;
			EnableColliderAfterDelay();
		}
		if (canFightMonster)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
			}
		}
	}

	private void EnableColliderAfterDelay()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		skillTF.up = Vector3.up;
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		speed = 0f;
		zhadanAnimation.AnimationState.SetAnimation(0, "2", true);
		zhadanAnimation.AnimationState.SetAnimation(1, "3", true);
		zhadanAnimation.AnimationState.AddEmptyAnimation(1, 0f, GetAttrValueByType(EAttrSkillType.Duration));
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = damageRange / 2f;
	}
}
