using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbHero : zbAttackerBase
{
	[NonSerialized]
	public BattleSkillDetail skillDetail;

	[NonSerialized]
	private ESkillType m_SkillType;

	public float nextAddCdTime;

	private bool m_lastSuperfActive;

	public virtual void InitData(zbCombatData _combatData, zbCombatTeam _team, BattleSkillDetail _skill)
	{
		spineColorChangeChe = new zbSpineColorChange();
		spineColorChangeChe.Init(((Component)((Component)this).transform.Find("empty").Find("zhuangjiache")).GetComponent<SkeletonRenderer>());
		nodeCarSuperef = zbUtil.FindGameObjectRecursive(((Component)this).transform, "carSuperef");
		combatData = _combatData;
		team = _team;
		skillDetail = _skill;
		m_SkillType = skillDetail.SkillType;
		animationArmoredDefenseTime = Rand.Range(1f, 2.5f);
		animationArmoredDefenseTimePre = 0f;
		_ = combatData.dataIndex;
		_ = 2;
		heroAttr = new zbHeroAttr();
		heroAttr.Init(this);
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(skillDetail.HeroTid);
		heroAvatarRecord = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
		skillTypeRecord = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey((int)m_SkillType);
		GameObject val = zbUtil.FindGameObjectRecursive(((Component)this).transform, "zhuangjiache");
		if ((Object)(object)val != (Object)null && zbCombatSystem.Instance.GetDefenseType() == EDefenseType.CarDefense)
		{
			animationArmoredDefense = val.GetComponent<SkeletonAnimation>();
			zbWallCsvRecord recordByKey2 = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(combatData.battleSideTeamInfo.MasterTeamInfo.DefenseInfo.WallId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(animationArmoredDefense, recordByKey2.carAnimation + "_SkeletonData.asset", "idle", loop: true);
			}
			animationArmoredDefense.AnimationState.SetAnimation(0, "idle", true);
			animationArmoredDefense.AnimationState.SetAnimation(1, "move", true);
			boneCarMove = ((SkeletonRenderer)animationArmoredDefense).skeleton.FindBone("a");
		}
		nodeCarSuperef.SetActive(false);
		nodeArmoredDefense = zbUtil.FindGameObjectRecursive(((Component)this).transform, "empty");
		if ((Object)(object)nodeArmoredDefense != (Object)null)
		{
			if (zbCombatSystem.Instance.GetDefenseType() != EDefenseType.CarDefense)
			{
				nodeArmoredDefense.SetActive(false);
			}
			else if (m_SkillType == ESkillType.Motuo)
			{
				nodeArmoredDefense.SetActive(false);
			}
			else
			{
				nodeArmoredDefense.SetActive(true);
				((Component)((Component)this).transform.Find("yingzi")).gameObject.SetActive(false);
			}
		}
		if (m_SkillType == ESkillType.Qiang)
		{
			nodeHeroAvatar = zbUtil.FindGameObjectRecursive(((Component)this).transform, "heroAvatar");
			nodeHeroAvatar.SetActive(false);
			nodeHeroAvatar = zbUtil.FindGameObjectRecursive(((Component)this).transform, "fightAvatar");
		}
		else
		{
			nodeHeroAvatar = zbUtil.FindGameObjectRecursive(((Component)this).transform, "fightAvatar");
			nodeHeroAvatar.SetActive(false);
			nodeHeroAvatar = zbUtil.FindGameObjectRecursive(((Component)this).transform, "heroAvatar");
		}
		anim = new zbFightAnimation();
		fsm = new zbHeroStateMachine();
		anim.Init(this);
		fsm.Init(this);
		RefreshRiseEffect(skillDetail.SkillId);
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		fsm.SetState(EHeroState.Show);
		SetIdleAnimation(loop: true);
		CreatePet();
	}

	public virtual void CreatePet()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		BattleServantDetail servantDetail = zbSelf.Instance.combatHandler.GetServantDetail(team.battleTeamInfo, GetSkillType());
		if (servantDetail != null)
		{
			GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/pet.prefab"));
			val.transform.SetParent(combatData.nodeRootHero);
			val.transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y - 10f, ((Component)this).transform.position.z);
			val.transform.localScale = Vector3.one;
			pet = ((Component)val.transform).GetComponent<zbPet>();
			pet.InitData(combatData, team, skillDetail, this, servantDetail);
		}
	}

	public virtual void Restore(List<stRecordHero> heroList, stRecordHeroQiang heroQiang = null)
	{
		foreach (stRecordHero hero in heroList)
		{
			if (hero.heroTid == GetHeroTid())
			{
				nextFireTime = hero.nextFireTime;
				needDurationProgress = hero.needDurationProgress;
				needDurationProgressMax = hero.needDurationProgressMax;
			}
		}
	}

	public virtual void OnAfterModCreated()
	{
	}

	public override int GetHeroTid()
	{
		return skillDetail.HeroTid;
	}

	public override int GetSkillType()
	{
		return (int)m_SkillType;
	}

	public int GetSkillId()
	{
		return skillRecord.id;
	}

	public virtual void RefreshRiseEffect(int skillId)
	{
		skillRecord = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(skillId);
		if (skillRecord != null)
		{
			if (skillRecord.riseEffectId == "")
			{
				riseEffect = null;
			}
			SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>("animation/effect/hero/" + skillRecord.bulletSpineName + "/" + skillRecord.bulletSpineName + "_SkeletonData.asset");
			if ((Object)(object)riseEffect != (Object)null && (Object)(object)val != (Object)null)
			{
				((SkeletonRenderer)riseEffect).skeletonDataAsset = val;
				((SkeletonRenderer)riseEffect).Initialize(true, false);
			}
			if (skillRecord.mod > 0)
			{
				team.modMgr.CreateMod(skillRecord.mod, 1);
			}
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected override void Update()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		fsm.Update();
		if (nodeArmoredDefense.activeSelf)
		{
			if (boneCarMove != null)
			{
				nodeHeroAvatar.transform.localPosition = Vector2.op_Implicit(new Vector2(0f, boneCarMove.WorldY));
				nodeCarSuperef.transform.localPosition = Vector2.op_Implicit(new Vector2(0f, boneCarMove.WorldY));
			}
			if (zbCombatSystem.Instance.scrollSpeed <= 0f)
			{
				animationArmoredDefense.timeScale = 0f;
			}
			bool flag = combatData.defense.immunityDuration > 0f;
			if (flag != m_lastSuperfActive)
			{
				m_lastSuperfActive = flag;
				nodeCarSuperef.SetActive(flag);
			}
			spineColorChangeChe.DeltaUpdate();
			animationArmoredDefenseTimePre += Time.deltaTime;
			if (animationArmoredDefenseTimePre > animationArmoredDefenseTime)
			{
				animationArmoredDefenseTime = Rand.Range(1f, 2.5f);
				animationArmoredDefenseTimePre = 0f;
				float timeScale = Rand.Range(8f, 12f) / 10f;
				animationArmoredDefense.timeScale = timeScale;
			}
		}
	}

	public void LerpChangeColor()
	{
		spineColorChangeChe.LerpChangeColor();
	}

	public float GetAlertRange()
	{
		return heroAttr.GetAttrValueByType(EAttrSkillType.AlertRange);
	}

	public virtual void AdjustHeroRotation(Vector3 targetPosition)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		TrackEntry val = anim.PlayTackAnimation(0, "attack", loop: false);
		anim.PlayTackAnimation(1, "aim", loop: true);
		Vector2 val2 = Vector2.op_Implicit(zbUtil.GetTargetNormalized(GetGunpointPosition(), targetPosition));
		float num = Vector2.Angle(Vector2.up, val2);
		if (targetPosition.x < combatData.nodeRootPositonX && num > 15f)
		{
			if (((SkeletonRenderer)anim.skeletonAnimation).skeleton.Data.FindAnimation("turn") != null)
			{
				anim.PlayTackAnimation(2, "turn", loop: false);
			}
		}
		else
		{
			anim.StopAnimation(2);
		}
		if (GetSkillType() != 1)
		{
			SetIdleAnimation(loop: true, val);
			anim.skeletonAnimation.AnimationState.AddEmptyAnimation(1, 0f, val.AnimationEnd);
		}
		Vector3 val3 = ((Component)anim.skeletonAnimation).transform.InverseTransformPoint(targetPosition);
		SkeletonExtensions.SetLocalPosition(boneShoot, val3);
		if ((Object)(object)riseEffect != (Object)null)
		{
			riseEffect.AnimationState.SetAnimation(0, skillRecord.riseEffectId, false);
		}
	}

	public override void SetIdleAnimation(bool loop, TrackEntry shootTrack = null)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (shootTrack != null)
		{
			anim.AddAnimation(0, "idle", loop, shootTrack.AnimationEnd);
		}
		else
		{
			anim.PlayTackAnimation(0, "idle", loop);
		}
		if (((Component)this).transform.position.x > 1f && ((SkeletonRenderer)anim.skeletonAnimation).skeleton.Data.FindAnimation("turn") != null)
		{
			anim.PlayTackAnimation(2, "turn", loop: false);
		}
	}
}
