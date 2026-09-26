using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using UnityEngine;

public class zbPetSkill0900 : zbPetSkillHandleBase
{
	private zbHero heroParent;

	private int monsterNumMax;

	private float cdMax;

	private int subSkillId;

	private float cd;

	private float alertRange;

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
		if (subSkillId == 40903)
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		}
		else
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/motuo.ogg");
		}
		int i = 0;
		Vector3 targetPosition = default(Vector3);
		while (i < 5)
		{
			((Vector3)(ref targetPosition))._002Ector(((float)i - 2f) * 1.4f, -5f, 0f);
			FireToTargetPosition(0, targetPosition);
			yield return (object)new WaitForSeconds(0.2f);
			int num = i + 1;
			i = num;
		}
		val = pet.skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Component)pet).transform.position = new Vector3(((Component)pet).transform.position.x, ((Component)hero).transform.position.y - 10f, ((Component)pet).transform.position.z);
	}

	public void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		zbSkillSystem.Instance.CreateSubSkill(subSkillId, hero, null, hero.heroAttr.skillAttrList, targetPosition, Vector2.up);
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
