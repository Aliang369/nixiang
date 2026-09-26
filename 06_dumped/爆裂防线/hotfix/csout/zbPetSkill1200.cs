using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill1200 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private int monsterNumMax;

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
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", true);
		Vector3 gunpointPosition = pet.GetGunpointPosition();
		ShootClosestMonster(gunpointPosition);
		yield return (object)new WaitForSeconds(subSkillDuration);
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public void ShootClosestMonster(Vector3 petPosition)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		zbMonster randomMonster = pet.combatData.monsters.GetRandomMonster();
		int mainMonsterUUID;
		Vector3 curPosition = default(Vector3);
		if ((Object)(object)randomMonster != (Object)null)
		{
			mainMonsterUUID = randomMonster.GetUUID();
			curPosition = randomMonster.GetCurPosition();
		}
		else
		{
			mainMonsterUUID = 0;
			((Vector3)(ref curPosition))._002Ector((float)Rand.Range(-3, 3), 7f, 0f);
		}
		ShootRepeater(petPosition, mainMonsterUUID, curPosition);
	}

	public void ShootRepeater(Vector3 petPosition, int mainMonsterUUID, Vector3 targetPosition)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_1.ogg");
		zbSub41201 zbSub41402 = (zbSub41201)zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, petPosition, Vector2.up);
		zbSub41402.setDianqiuMonsterId(mainMonsterUUID, targetPosition, null, 0);
		subSkillDuration = zbSub41402.GetAttrValueByType(EAttrSkillType.Duration);
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
