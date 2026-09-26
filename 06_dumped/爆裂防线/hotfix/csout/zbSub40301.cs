using Proto.Enums;
using UnityEngine;

public class zbSub40301 : zbSkillBase
{
	private float laserWidth = 0.17f;

	private LineRenderer lineRenderer;

	private AudioSource m_audio;

	private GameObject gameObjectAnimation;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float duration;

	protected override void OnInited()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		targetMonsterUUID = mainMonsterUUID;
		gameObjectAnimation = zbUtil.FindGameObjectRecursive(((Component)this).transform, "TargetAnimation1");
		lineRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)lineRenderer == (Object)null)
		{
			lineRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		lineRenderer.startWidth = laserWidth;
		lineRenderer.endWidth = laserWidth;
		lineRenderer.positionCount = 2;
		m_audio = null;
		lineRenderer.SetPosition(0, ((Component)this).transform.position);
		duration = GetAttrValueByType(EAttrSkillType.Duration);
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		needDurationPre = 0f;
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/jiguang.ogg", loop: true);
	}

	private void UpdateLaser()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if (!((Object)(object)monsterById == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(monsterById.GetCurPosition());
			((Component)this).transform.position = Vector2.op_Implicit(val);
			lineRenderer.SetPosition(1, Vector2.op_Implicit(val));
		}
	}

	protected override void Update()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null && monsterById.isCanSelected())
		{
			UpdateLaser();
		}
		else
		{
			zbMonster closestMonsterByVector = combatData.monsters.GetClosestMonsterByVector(lineRenderer.GetPosition(1));
			if ((Object)(object)closestMonsterByVector != (Object)null)
			{
				targetMonsterUUID = closestMonsterByVector.GetUUID();
				UpdateLaser();
			}
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= duration)
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
	}

	private void StopLoopAudio()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
	}

	private void DelayDestroySelf()
	{
		StopLoopAudio();
		ReturnToPool();
	}

	private void UpdateMonsterCollision()
	{
		ShootFormulaMonster(targetMonsterUUID);
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
	}
}
