using System.Collections.Generic;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbBulletLongjuan : zbSkillBase
{
	private Vector3 targetP;

	private float bulletSpeed = 1f;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_skillDuration;

	private zbBoundsCircle2D boundsCircle2D;

	private int excludeUUID;

	private int selfHitNumber;

	protected static readonly List<Vector3> targetVector3Up;

	protected static readonly List<Vector3> targetVector3Bm;

	protected override void OnInited()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		selfHitNumber = 0;
		needDurationPre = 0f;
		targetP = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		bulletSpeed = GetAttrValueByType(EAttrSkillType.MoveSpeed);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		skillTF.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		conitnueNext();
		m_damageIntervalPre = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		m_skillDuration = GetAttrValueByType(EAttrSkillType.Duration);
		m_skeletonAnimation.UpdateTiming = (UpdateTiming)(!zbCombatSystem.Instance.combatFPS.IsVeryLowPerfomanceMode());
	}

	protected void conitnueNext()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		zbMonster randomMonster = combatData.monsters.GetRandomMonster(excludeUUID);
		if ((Object)(object)randomMonster != (Object)null)
		{
			targetP = ((Component)randomMonster).transform.position;
			excludeUUID = randomMonster.GetUUID();
			return;
		}
		if (skillTF.position.y >= 0f)
		{
			targetP = targetVector3Bm[Rand.Range(0, 3)];
		}
		else
		{
			targetP = targetVector3Up[Rand.Range(0, 3)];
		}
		excludeUUID = 0;
	}

	private void DelayDestroySelf()
	{
		ReturnToPool();
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
		needDurationPre += Time.deltaTime;
		if (needDurationPre >= m_skillDuration + hero.needDurationProgressMaxTemp)
		{
			needDurationPre = 0f;
			DelayDestroySelf();
		}
		if (zbUtil.Reached(Vector2.op_Implicit(skillTF.position), Vector2.op_Implicit(targetP)))
		{
			conitnueNext();
			return;
		}
		Vector3 position = skillTF.position;
		Vector2 val = Vector2.MoveTowards(Vector2.op_Implicit(position), Vector2.op_Implicit(targetP), bulletSpeed * Time.deltaTime);
		position.x = val.x;
		position.y = val.y;
		skillTF.position = position;
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = skillTF.localScale.x / 2f;
		m_damageIntervalPre += Time.deltaTime;
		if (!(m_damageIntervalPre >= m_damageInterval))
		{
			return;
		}
		m_damageIntervalPre -= m_damageInterval;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		bool flag = true;
		foreach (stModIndex2134 item in hero.team.modIndex2134)
		{
			selfHitNumber++;
			if (selfHitNumber >= item.releaseNumber)
			{
				if (flag)
				{
					combatData.monsters.GetRandomMonsterList(5, monsterRandomList);
					flag = false;
				}
				for (int i = 0; i < item.extraNumber; i++)
				{
					zbSkillSystem.Instance.CreateSubSkill(item.subSkillId, hero, this, skillAttrList, skillTF.position, Vector2.up, 0, (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
				}
				selfHitNumber = 0;
			}
		}
	}

	static zbBulletLongjuan()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		targetVector3Up = new List<Vector3>
		{
			new Vector3(-2f, 5f),
			new Vector3(2f, 5f),
			new Vector3(0f, 5f)
		};
		targetVector3Bm = new List<Vector3>
		{
			new Vector3(-2f, -5f),
			new Vector3(2f, -5f),
			new Vector3(0f, -5f)
		};
	}
}
