using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill0700 : zbPetSkillHandleBase
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
		int repeat = 0;
		while (repeat < bulletNumMax)
		{
			yield return ShootClosestMonster();
			int num = repeat + 1;
			repeat = num;
		}
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public IEnumerator ShootClosestMonster()
	{
		zbMonster closestMonster = pet.combatData.monsters.GetClosestMonster(100f);
		int mainMonsterUUID;
		Vector3 curPosition = default(Vector3);
		if ((Object)(object)closestMonster != (Object)null)
		{
			mainMonsterUUID = closestMonster.GetUUID();
			curPosition = closestMonster.GetCurPosition();
		}
		else
		{
			mainMonsterUUID = 0;
			((Vector3)(ref curPosition))._002Ector((float)Rand.Range(-3, 3), 7f, 0f);
		}
		yield return ShootRepeater(curPosition, mainMonsterUUID);
	}

	public IEnumerator ShootRepeater(Vector3 targetPosition, int mainMonsterUUID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/diancichuanci.ogg");
		zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, targetPosition, Vector2.up, mainMonsterUUID);
		yield return (object)new WaitForSeconds(0.1f);
	}

	public override void Update()
	{
		base.Update();
		cd += Time.deltaTime;
	}

	public override void TriggerKill()
	{
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
