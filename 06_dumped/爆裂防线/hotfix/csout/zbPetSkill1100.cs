using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill1100 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private float durationTime;

	private int bulletNumMax;

	private int subSkillId;

	private float subSkillDuration;

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
		pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_loop", true);
		Vector3 gunpointPosition = pet.GetGunpointPosition();
		for (int i = 0; i < bulletNumMax; i++)
		{
			ShootClosestMonster(gunpointPosition);
		}
		yield return (object)new WaitForSeconds(subSkillDuration);
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public void ShootClosestMonster(Vector3 petPosition)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
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
		ShootRepeater(petPosition, mainMonsterUUID);
	}

	public void ShootRepeater(Vector3 petPosition, int mainMonsterUUID)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		zbSkillBase zbSkillBase2 = zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, petPosition, Vector2.up, mainMonsterUUID);
		subSkillDuration = zbSkillBase2.GetAttrValueByType(EAttrSkillType.Duration);
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
