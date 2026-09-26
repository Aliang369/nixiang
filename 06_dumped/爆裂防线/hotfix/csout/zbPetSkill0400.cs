using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill0400 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private int bulletNumMax;

	private float cdMax;

	private int subSkillId;

	private float cd;

	private float alertRange;

	public override void Initialize(zbHero _hero, zbServantSkillCsvRecord _servantSkillRecord, zbPet _pet, BattleServantSkillDetail _servantSkillDetail)
	{
		base.Initialize(_hero, _servantSkillRecord, _pet, _servantSkillDetail);
		heroParent = hero;
		int num = 0;
		bulletNumMax = (int)_servantSkillRecord.param[num];
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
		Vector3 petPosition = pet.GetGunpointPosition();
		int repeat = 0;
		while (repeat < bulletNumMax)
		{
			yield return ShootClosestMonster(petPosition);
			int num = repeat + 1;
			repeat = num;
		}
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public IEnumerator ShootClosestMonster(Vector3 petPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
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
		Vector2 directionToTarget = Vector2.op_Implicit(zbUtil.GetTargetNormalized(petPosition, curPosition));
		yield return ShootRepeater(directionToTarget, petPosition);
	}

	public IEnumerator ShootRepeater(Vector2 directionToTarget, Vector3 petPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/ganbingdan_fire.ogg");
		zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, petPosition, directionToTarget);
		yield return (object)new WaitForSeconds(0.1f);
	}

	public override void Update()
	{
		base.Update();
		cd += Time.deltaTime;
		if (cd >= cdMax)
		{
			if (alertRange <= 0f)
			{
				alertRange = heroParent.GetAttrValueByType(EAttrSkillType.AlertRange);
			}
			if (pet.combatData.monsters.HasMonsterInAlertRange(alertRange))
			{
				cd = 0f;
				((MonoBehaviour)pet).StartCoroutine(ActionSkill());
			}
		}
	}
}
