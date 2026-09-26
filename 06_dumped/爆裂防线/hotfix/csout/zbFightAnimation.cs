using Spine;
using Spine.Unity;
using UnityEngine;
using zbType;

public class zbFightAnimation
{
	private zbAttackerBase m_mono;

	public SkeletonAnimation skeletonAnimation;

	private SkeletonAnimation m_spineGun;

	public void Init(zbAttackerBase owner)
	{
		m_mono = owner;
		if (m_mono.GetFightPeopleType() == EFightPeopleType.Hero && m_mono.GetSkillType() == 1)
		{
			GameObject gameObject = ((Component)((Component)m_mono).transform.Find("fightAvatar").Find("avatarBody")).gameObject;
			skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
			GameObject gameObject2 = ((Component)((Component)m_mono).transform.Find("fightAvatar").Find("gun")).gameObject;
			m_spineGun = gameObject2.GetComponent<SkeletonAnimation>();
			m_mono.goRiseEffect = ((Component)((Component)m_mono).transform.Find("fightAvatar").Find("riseEffect")).gameObject;
		}
		else
		{
			GameObject gameObject3 = ((Component)((Component)m_mono).transform.Find("heroAvatar")).gameObject;
			skeletonAnimation = gameObject3.GetComponent<SkeletonAnimation>();
			m_mono.goRiseEffect = ((Component)((Component)m_mono).transform.Find("heroAvatar").Find("riseEffect")).gameObject;
		}
		m_mono.riseEffect = m_mono.goRiseEffect.GetComponent<SkeletonAnimation>();
		m_mono.goRiseEffect.SetActive(false);
		RefreshSpine();
	}

	public void Free()
	{
	}

	public TrackEntry PlayAnimation(string animationName, bool loop)
	{
		if (((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animationName) == null)
		{
			return null;
		}
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			return skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		}
		return null;
	}

	public void AddAnimation(int tackIndex, string animationName, bool loop, float dtime)
	{
		if (((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animationName) != null && (Object)(object)skeletonAnimation != (Object)null)
		{
			skeletonAnimation.AnimationState.AddAnimation(tackIndex, animationName, loop, dtime);
		}
	}

	public void StopAnimation(int tackIndex)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(tackIndex);
			if (current == null || current.Animation.Name != "<empty>")
			{
				skeletonAnimation.AnimationState.SetEmptyAnimation(tackIndex, 0f);
			}
		}
	}

	public TrackEntry PlayTackAnimation(int tackIndex, string animationName, bool loop)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(tackIndex);
			if (current != null && ((!current.Loop && current.AnimationTime < current.AnimationEnd) || current.Loop) && current.Animation.Name == animationName)
			{
				current.Loop = loop;
				return current;
			}
			return skeletonAnimation.AnimationState.SetAnimation(tackIndex, animationName, loop);
		}
		return null;
	}

	public void RefreshSpine()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		if (m_mono.GetFightPeopleType() == EFightPeopleType.Mecha)
		{
			_ = m_mono.mechaAvatarRecord.fightBone + "_SkeletonData.asset";
			skeletonAnimation = ((Component)m_mono).GetComponentInChildren<SkeletonAnimation>();
			zbUtil.RefreshSpine(skeletonAnimation, m_mono.mechaAvatarRecord.fightBone + "_SkeletonData.asset", "idle", loop: true);
			if (m_mono.combatData.dataIndex == 1)
			{
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			}
			m_mono.gunpoint = ((SkeletonRenderer)skeletonAnimation).skeleton.FindBone(zbCombatDefine.DUMMY_GUNPOINT);
		}
		else if (m_mono.GetSkillType() == 1)
		{
			int avatarId = m_mono.team.battleTeamInfo.RoleBaseInfo.AvatarId;
			if (m_mono.combatData.dataIndex == 1)
			{
				zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
				zbPlayerGunCsvRecord recordByKey2 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_mono.team.battleTeamInfo.RoleBaseInfo.GunId);
				zbUtil.RefreshSpine(skeletonAnimation, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
				zbUtil.RefreshSpine(m_spineGun, recordByKey2.fightBone + "_SkeletonData.asset", "animation", loop: true);
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				((Component)m_mono).transform.localScale = Vector3.one;
			}
			else
			{
				zbHeroAvatarCsvRecord recordByKey3 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
				zbPlayerGunCsvRecord recordByKey4 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_mono.team.battleTeamInfo.RoleBaseInfo.GunId);
				zbUtil.RefreshSpine(skeletonAnimation, recordByKey3.frontBone + "_SkeletonData.asset", "idle", loop: true);
				zbUtil.RefreshSpine(m_spineGun, recordByKey4.frontBone + "_SkeletonData.asset", "animation", loop: true);
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				((Component)m_mono).transform.localPosition = new Vector3(((Component)m_mono).transform.localPosition.x, -6.2f, 1f);
				((Component)m_mono).transform.localScale = Vector3.one * 0.25f;
			}
			m_mono.gunpoint = ((SkeletonRenderer)m_spineGun).skeleton.FindBone(zbCombatDefine.DUMMY_GUNPOINT);
		}
		else
		{
			skeletonAnimation = ((Component)m_mono).GetComponentInChildren<SkeletonAnimation>();
			if (m_mono.combatData.dataIndex == 1)
			{
				zbUtil.RefreshSpine(skeletonAnimation, m_mono.heroAvatarRecord.fightBone + "_SkeletonData.asset", "idle", loop: true);
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				((Component)m_mono).transform.localScale = Vector3.one;
			}
			else
			{
				zbUtil.RefreshSpine(skeletonAnimation, m_mono.heroAvatarRecord.frontBone + "_SkeletonData.asset", "idle", loop: true);
				((Component)m_mono).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				((Component)m_mono).transform.localPosition = new Vector3(((Component)m_mono).transform.localPosition.x, -6.2f, ((Component)m_mono).transform.localPosition.z);
				((Component)m_mono).transform.localScale = (Vector3.one + Vector3.left * 2f) * 0.8f * 0.94f;
			}
			m_mono.gunpoint = ((SkeletonRenderer)skeletonAnimation).skeleton.FindBone(zbCombatDefine.DUMMY_GUNPOINT);
		}
		m_mono.boneShoot = ((SkeletonRenderer)skeletonAnimation).skeleton.FindBone("shoot");
	}
}
