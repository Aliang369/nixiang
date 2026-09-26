using System.Collections;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill0800 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private float durationTime;

	private int bulletNumMax;

	private int subSkillId;

	public override void Initialize(zbHero _hero, zbServantSkillCsvRecord _servantSkillRecord, zbPet _pet, BattleServantSkillDetail _servantSkillDetail)
	{
		base.Initialize(_hero, _servantSkillRecord, _pet, _servantSkillDetail);
		heroParent = hero;
		int num = 0;
		durationTime = _servantSkillRecord.param[num];
		num++;
		bulletNumMax = (int)_servantSkillRecord.param[num];
		num++;
		subSkillId = _servantSkillRecord.subSkillId;
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
		zbMonster randomMonster = pet.combatData.monsters.GetRandomMonster();
		int mainMonsterUUID;
		if ((Object)(object)randomMonster != (Object)null)
		{
			mainMonsterUUID = randomMonster.GetUUID();
			randomMonster.GetCurPosition();
		}
		else
		{
			mainMonsterUUID = 0;
			new Vector3((float)Rand.Range(-3, 3), 7f, 0f);
		}
		yield return ShootRepeater(petPosition, mainMonsterUUID);
	}

	public IEnumerator ShootRepeater(Vector3 petPosition, int mainMonsterUUID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", false);
		zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, petPosition, Vector2.up, mainMonsterUUID);
		yield return (object)new WaitForSeconds(0.1f);
	}

	public override void Update()
	{
		base.Update();
		if (heroParent.goingOnHowLong >= durationTime)
		{
			heroParent.goingOnHowLong = 0f;
			((MonoBehaviour)pet).StartCoroutine(ActionSkill());
		}
	}
}
