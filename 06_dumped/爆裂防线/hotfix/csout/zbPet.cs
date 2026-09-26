using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbPet : zbAttackerBase
{
	[NonSerialized]
	public BattleSkillDetail skillDetail;

	[NonSerialized]
	private ESkillType m_SkillType;

	public SkeletonAnimation skeletonAnimation;

	private zbServantAvatarCsvRecord servantAvatarRecord;

	private zbHero hero;

	private zbPetSkillHandleBase petSkillHandler;

	public virtual void InitData(zbCombatData _combatData, zbCombatTeam _team, BattleSkillDetail _skill, zbHero _hero, BattleServantDetail _battleServantDetail)
	{
		hero = _hero;
		combatData = _combatData;
		team = _team;
		skillDetail = _skill;
		m_SkillType = skillDetail.SkillType;
		skeletonAnimation = ((Component)((Component)this).transform.Find("heroAvatar")).gameObject.GetComponent<SkeletonAnimation>();
		goRiseEffect = ((Component)((Component)this).transform.Find("heroAvatar").Find("riseEffect")).gameObject;
		m_sortType = ESortLayerType.HeroFront;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		_hero.GetSkillType();
		GetPetDetail();
		zbServantCsvRecord recordByKey = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(_battleServantDetail.ServantTid);
		servantAvatarRecord = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
		zbServantSkillCsvRecord recordByKey2 = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(recordByKey.skillId);
		string spinePath = servantAvatarRecord.fightBone + "_SkeletonData.asset";
		zbUtil.RefreshSpine(skeletonAnimation, spinePath, "idle", loop: true);
		skeletonAnimation.AnimationState.SetAnimation(0, "attack_out", false);
		petSkillHandler = combatData.petRegister.CreatePetSkillHandler(recordByKey.skillId);
		if (petSkillHandler != null)
		{
			petSkillHandler.Initialize(hero, recordByKey2, this, null);
		}
	}

	public BattleServantDetail GetPetDetail()
	{
		foreach (BattleServantDetail servant in team.battleTeamInfo.ServantList)
		{
			if (servant.SkillType == (int)m_SkillType)
			{
				return servant;
			}
		}
		return null;
	}

	public void TriggerKill()
	{
		if (petSkillHandler != null)
		{
			petSkillHandler.TriggerKill();
		}
	}

	public void TriggerAttacked()
	{
		if (petSkillHandler != null)
		{
			petSkillHandler.TriggerAttacked();
		}
	}

	public override int GetHeroTid()
	{
		return skillDetail.HeroTid;
	}

	public override int GetSkillType()
	{
		return (int)m_SkillType;
	}

	protected override void Update()
	{
		petSkillHandler.Update();
	}
}
