using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbHeroMain : zbHero
{
	public override void OnSkillUpdate()
	{
		ExecSkillUpdate();
	}

	public void ExecSkillUpdate()
	{
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		if (needDurationFlag && nextFireTime <= 0f && combatData.skillPoolMgr.m_pool.TryGetValue(skillTypeRecord.bulletPrefab, out var value) && value.m_activeSkills.Count <= 0)
		{
			needDurationProgress = 0f;
		}
		if (zbCombatSystem.Instance.m_isCelebrating || !(needDurationProgress <= 0f))
		{
			return;
		}
		if (nextAddCdTime > 0f)
		{
			nextFireTime -= nextAddCdTime;
			nextAddCdTime = 0f;
		}
		nextFireTime = Time.deltaTime + nextFireTime;
		if (!(nextFireTime >= GetAttrValueByType(EAttrSkillType.Cd)) || isCDFlag || !combatData.monsters.HasMonsterInAlertRange(GetAttrValueByType(EAttrSkillType.AlertRange)))
		{
			return;
		}
		isCDFlag = true;
		team.Trigger1082(this);
		((MonoBehaviour)this).StartCoroutine(ShootMonster());
		combatData.monsters.GetRandomMonsterList(10, monsterRandomList);
		foreach (stModIndex2021 item in team.modIndex2021)
		{
			if (item.skillType != GetSkillType())
			{
				continue;
			}
			item.tickNumber++;
			if (item.tickNumber < item.releaseNumber)
			{
				continue;
			}
			if (zbUtil.Roll0to1(item.roll0to1))
			{
				for (int i = 0; i < item.extraNumber; i++)
				{
					if (monsterRandomList.Count > i)
					{
						FireToTargetPosition(monsterRandomList[i].GetUUID(), monsterRandomList[i].GetCurPosition());
					}
					else
					{
						((MonoBehaviour)this).StartCoroutine(ShootRandomMonster());
					}
				}
			}
			item.tickNumber = 0;
		}
		foreach (stModIndex1037 item2 in team.modIndex1037)
		{
			if (item2.skillType != GetSkillType())
			{
				continue;
			}
			item2.tickNumber++;
			if (item2.tickNumber >= item2.releaseNumber && item2.skillType == GetSkillType())
			{
				for (int j = 0; j < item2.extraNumber; j++)
				{
					zbSkillSystem.Instance.CreateSubSkill(item2.subSkillId, this, null, heroAttr.skillAttrList, GetGunpointPosition(), Vector2.up, 0, (monsterRandomList.Count > j) ? monsterRandomList[j].GetUUID() : 0);
				}
				item2.tickNumber = 0;
			}
		}
		if (GetSkillType() != 6)
		{
			return;
		}
		foreach (stModIndex1064 item3 in team.modIndex1064)
		{
			item3.violentRateAdd = item3.violentRateBase * (1f + GetAttrValueByType(EAttrSkillType.Extra));
		}
	}

	public override IEnumerator ShootMonster()
	{
		((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
		while (isCDFlag)
		{
			yield return null;
		}
		int i = 0;
		while ((float)i < GetAttrValueByType(EAttrSkillType.Extra))
		{
			yield return (object)new WaitForSeconds(extraBulletInterval);
			((MonoBehaviour)this).StartCoroutine(ShootRandomMonster());
			int num = i + 1;
			i = num;
		}
	}

	public override IEnumerator ShootClosestMonster()
	{
		zbMonster targetMonster = combatData.monsters.GetClosestMonster(GetAttrValueByType(EAttrSkillType.AlertRange));
		if (Object.op_Implicit((Object)(object)targetMonster))
		{
			int targetMonsterId = targetMonster.GetUUID();
			if (zbCombatDefine.needAdjustHeroRotation.Contains(skillDetail.SkillType))
			{
				AdjustHeroRotation(targetMonster.GetCurPosition());
				yield return null;
			}
			else
			{
				TrackEntry shootTrack = anim.skeletonAnimation.AnimationState.SetAnimation(0, "attack", false);
				SetIdleAnimation(loop: true, shootTrack);
			}
			if ((Object)(object)combatData.monsters.GetMonsterById(targetMonsterId) != (Object)null)
			{
				SetFirst(_isFirst: true);
				FireToTargetPosition(targetMonster.GetUUID(), targetMonster.GetCurPosition());
				isCDFlag = false;
				nextFireTime = 0f;
			}
			else
			{
				yield return null;
				((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
			}
		}
		else
		{
			yield return null;
			((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
		}
	}

	public virtual IEnumerator ShootRandomMonster()
	{
		zbMonster randomMonster = combatData.monsters.GetRandomMonster();
		int targetMonsterId = 0;
		Vector3 vector = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
		if (Object.op_Implicit((Object)(object)randomMonster))
		{
			targetMonsterId = randomMonster.GetUUID();
			vector = randomMonster.GetCurPosition();
		}
		if (zbCombatDefine.needAdjustHeroRotation.Contains(skillDetail.SkillType))
		{
			AdjustHeroRotation(vector);
			yield return null;
			yield return null;
		}
		SetFirst(_isFirst: false);
		FireToTargetPosition(targetMonsterId, vector);
		yield return null;
	}

	public virtual void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
	}

	public virtual void SetFirst(bool _isFirst)
	{
	}
}
