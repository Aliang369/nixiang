using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using zbType;

public class zbAttackerBase : MonoBehaviour
{
	[NonSerialized]
	public zbPet pet;

	[NonSerialized]
	public zbCombatTeam team;

	[NonSerialized]
	public GameObject goRiseEffect;

	[NonSerialized]
	public zbCombatData combatData;

	[NonSerialized]
	public zbSkillTypeCsvRecord skillTypeRecord;

	[NonSerialized]
	public zbSkillCsvRecord skillRecord;

	[NonSerialized]
	public zbHeroAvatarCsvRecord heroAvatarRecord;

	[NonSerialized]
	public zbMechaAvatarCsvRecord mechaAvatarRecord;

	[NonSerialized]
	public zbHeroAttr heroAttr;

	[NonSerialized]
	public Bone boneShoot;

	[NonSerialized]
	protected Bone boneCarMove;

	protected SkeletonAnimation animationArmoredDefense;

	[NonSerialized]
	public float nextFireTime = 999f;

	[NonSerialized]
	public GameObject nodeArmoredDefense;

	[NonSerialized]
	public GameObject nodeHeroAvatar;

	[NonSerialized]
	public GameObject nodeCarSuperef;

	[NonSerialized]
	public zbFightAnimation anim;

	[NonSerialized]
	public SkeletonAnimation riseEffect;

	[NonSerialized]
	public zbHeroStateMachine fsm;

	[NonSerialized]
	public ESortLayerType m_sortType = ESortLayerType.Hero;

	[NonSerialized]
	protected float spreadAngle = 5f;

	[NonSerialized]
	public Bone gunpoint;

	[NonSerialized]
	public bool needDurationFlag;

	[NonSerialized]
	public float needDurationProgress;

	[NonSerialized]
	public float needDurationProgressMax = 1f;

	[NonSerialized]
	public float needDurationProgressMaxTemp;

	[NonSerialized]
	public float extraBulletInterval = 7f / 30f;

	protected zbSpineColorChange spineColorChangeChe;

	protected float animationArmoredDefenseTime;

	protected float animationArmoredDefenseTimePre;

	[NonSerialized]
	public bool isCDFlag;

	[NonSerialized]
	public List<zbMonster> monsterRandomList = new List<zbMonster>();

	public HashSet<int> hitMonsterList = new HashSet<int>();

	private EFightPeopleType peoleType;

	public float goingOnHowLong;

	public float GetAttrValueByType(EAttrSkillType attrSkillType)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		float num = heroAttr.GetAttrValueByType(attrSkillType);
		if (EAttrSkillType.Extra == attrSkillType)
		{
			float num2 = team.ExtraProperties[ETeamExtra.GemMod1058_1];
			if (num2 > 0f && zbUtil.Roll0to1(num2))
			{
				num += (float)(int)team.ExtraProperties[ETeamExtra.GemMod1058_2];
			}
		}
		if (EAttrSkillType.Cd == attrSkillType)
		{
			foreach (stModIndex1041 item in team.modIndex1041)
			{
				if (item.skillType == GetSkillType())
				{
					num *= 1f - item.cdDecRatio;
				}
			}
		}
		if (EAttrSkillType.AlertRange == attrSkillType)
		{
			if (zbCombatSystem.Instance.isPVPBattleType)
			{
				num = Math.Min(((Component)combatData.combatScene.top).transform.position.y - combatData.defense.GetAlertPositionY() - 1f, num);
			}
			num = Math.Min(combatData.GetSceneAlertRange(), num);
		}
		foreach (stModIndex1080 item2 in team.modIndex1080)
		{
			if (item2.attrSkillType == (int)attrSkillType)
			{
				zbSkillPool zbSkillPool2 = combatData.skillPoolMgr.FindSkillPool(item2.subSkillId);
				if (zbSkillPool2 != null)
				{
					num *= 1f + item2.attrValue * (float)zbSkillPool2.m_activeSkills.Count;
				}
			}
		}
		return num;
	}

	public virtual IEnumerator ShootMonster()
	{
		yield return null;
	}

	public virtual IEnumerator ShootClosestMonster()
	{
		yield return null;
	}

	public virtual int GetHeroTid()
	{
		return 0;
	}

	protected virtual void Update()
	{
	}

	public virtual int GetSkillType()
	{
		return 0;
	}

	public virtual void SetFightPeopleType(EFightPeopleType _peopleType)
	{
		peoleType = _peopleType;
	}

	public virtual EFightPeopleType GetFightPeopleType()
	{
		return peoleType;
	}

	public Vector3 GetGunpointPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return goRiseEffect.transform.position;
	}

	public virtual void SetIdleAnimation(bool loop, TrackEntry shootTrack = null)
	{
	}

	public virtual void OnSkillUpdate()
	{
	}
}
