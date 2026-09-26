using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill1400 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private int monsterNumMax;

	private int volleyMax;

	private int repeaterMax;

	private float cdMax;

	private int subSkillId;

	private float cd;

	private float alertRange;

	private float subSkillDuration;

	public override void Initialize(zbHero _hero, zbServantSkillCsvRecord _servantSkillRecord, zbPet _pet, BattleServantSkillDetail _servantSkillDetail)
	{
		base.Initialize(_hero, _servantSkillRecord, _pet, _servantSkillDetail);
		heroParent = hero;
		int num = 0;
		monsterNumMax = (int)_servantSkillRecord.param[num];
		num++;
		volleyMax = (int)_servantSkillRecord.param[num];
		num++;
		repeaterMax = (int)_servantSkillRecord.param[num];
		num++;
		cdMax = _servantSkillRecord.param[num];
		num++;
		subSkillId = _servantSkillRecord.subSkillId;
		cd = cdMax;
	}

	public IEnumerator ActionSkill()
	{
		((Component)pet).transform.position = new Vector3(((Component)hero).transform.position.x + 0.5f, ((Component)hero).transform.position.y, ((Component)pet).transform.position.z);
		TrackEntry val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_in", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		zbMonster closestMonster = pet.combatData.monsters.GetClosestMonster(100f);
		Vector3 curPosition = default(Vector3);
		if ((Object)(object)closestMonster != (Object)null)
		{
			curPosition = closestMonster.GetCurPosition();
		}
		else
		{
			((Vector3)(ref curPosition))._002Ector((float)Rand.Range(-3, 3), 7f, 0f);
		}
		Vector3 petPosition = pet.GetGunpointPosition();
		Vector2 directionToTarget = Vector2.op_Implicit(zbUtil.GetTargetNormalized(petPosition, curPosition));
		int repeat = 0;
		while (repeat < repeaterMax + 1)
		{
			yield return ShootRepeater(directionToTarget, petPosition, repeat);
			int num = repeat + 1;
			repeat = num;
		}
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public IEnumerator ShootRepeater(Vector2 directionToTarget, Vector3 offsetPosition, int number)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", false);
		yield return (object)new WaitForSeconds((float)number * 0.1f);
		zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
		for (int i = 0; i < volleyMax + 1; i++)
		{
			int num = (int)Math.Ceiling((float)i / 2f);
			float num2 = spreadAngle * (float)num;
			if (i % 2 == 1)
			{
				num2 = (0f - spreadAngle) * (float)num;
			}
			Vector2 dir = Vector2.op_Implicit(Quaternion.Euler(0f, 0f, num2) * Vector2.op_Implicit(directionToTarget));
			zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, offsetPosition, dir);
		}
	}

	public override void Update()
	{
		base.Update();
		m_refreshTimer += Time.unscaledDeltaTime;
		cd += Time.deltaTime;
		if (cd >= cdMax)
		{
			if (alertRange <= 0f)
			{
				alertRange = heroParent.GetAttrValueByType(EAttrSkillType.AlertRange);
			}
			if (m_refreshTimer >= zbPetSkillHandleBase.m_refreshInterval)
			{
				m_refreshTimer = 0f;
				UpdatePerSecond();
			}
		}
	}

	protected virtual void UpdatePerSecond()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		float num2 = pet.combatData.monsters.GetAlertRange(alertRange);
		foreach (zbMonster value in pet.combatData.monsters.monsterList.Values)
		{
			if (value.isCanSelected() && value.monsterTF.position.y <= num2)
			{
				num++;
				if (num >= monsterNumMax)
				{
					cd = 0f;
					((MonoBehaviour)pet).StartCoroutine(ActionSkill());
					break;
				}
			}
		}
	}
}
