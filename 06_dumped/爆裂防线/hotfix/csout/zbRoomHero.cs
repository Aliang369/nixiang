using System;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbRoomHero : MonoBehaviour
{
	[NonSerialized]
	public zbRoomHeroStateMachine fsm;

	private zbPlayerAvatar m_spineAvatar;

	private SkeletonAnimation m_skeletonAnimation;

	private SkeletonAnimation m_skeletonTrial;

	private static GameObject s_playerAvatarPrefab;

	[NonSerialized]
	public int curHeroTid;

	[NonSerialized]
	public WarRoomDetail curWarRoomDetail;

	[NonSerialized]
	public ESortMainLayerType m_sortType;

	private Transform visibleObject;

	private Vector2 vector2;

	protected void Awake()
	{
		fsm = new zbRoomHeroStateMachine();
		fsm.Init(this);
		visibleObject = ((Component)this).transform.Find("visibleObject");
		m_skeletonAnimation = ((Component)visibleObject.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_skeletonTrial = ((Component)visibleObject.Find("spineTrial")).GetComponent<SkeletonAnimation>();
		((Component)m_skeletonTrial).gameObject.SetActive(false);
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	protected void OnEnable()
	{
		fsm.OnEnable();
	}

	protected void OnDestroy()
	{
		ReleasePlayerAvatar();
	}

	protected void Update()
	{
		fsm.Update();
		UpdateZorder();
		UpdateTrial();
	}

	private void UpdateZorder()
	{
		zbSortMainMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void SetFacing(bool left)
	{
		if (GetSkillType() == ESkillType.Qiang)
		{
			m_spineAvatar?.SetFacing(left);
		}
		else if (left)
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = 1f;
		}
		else
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = -1f;
		}
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		if (GetSkillType() == ESkillType.Qiang)
		{
			return m_spineAvatar.SetAnimation(0, animationName, loop);
		}
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	private void EnsurePlayerAvatar()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_spineAvatar != (Object)null))
		{
			if ((Object)(object)s_playerAvatarPrefab == (Object)null)
			{
				s_playerAvatarPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/wnd/leader/playerAvatar.prefab");
			}
			if ((Object)(object)s_playerAvatarPrefab != (Object)null)
			{
				GameObject val = Object.Instantiate<GameObject>(s_playerAvatarPrefab, visibleObject);
				val.transform.localPosition = Vector3.zero;
				val.transform.localRotation = Quaternion.identity;
				val.transform.localScale = Vector3.one * 0.21f;
				m_spineAvatar = val.GetComponent<zbPlayerAvatar>();
			}
		}
	}

	private void ReleasePlayerAvatar()
	{
		if ((Object)(object)m_spineAvatar != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)m_spineAvatar).gameObject);
			m_spineAvatar = null;
		}
	}

	public void Refresh(int heroTid)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		curHeroTid = heroTid;
		if (heroTid > 0)
		{
			curWarRoomDetail = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(heroTid);
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroTid);
			if (recordByKey != null)
			{
				if (GetSkillType() == ESkillType.Qiang)
				{
					EnsurePlayerAvatar();
					if ((Object)(object)m_spineAvatar != (Object)null)
					{
						((Component)m_spineAvatar).gameObject.SetActive(true);
					}
					((Component)m_skeletonAnimation).gameObject.SetActive(false);
					m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
				}
				else
				{
					ReleasePlayerAvatar();
					((Component)m_skeletonAnimation).gameObject.SetActive(true);
					zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
					if (recordByKey2 != null)
					{
						string spinePath = recordByKey2.frontBone + "_SkeletonData.asset";
						zbUtil.RefreshSpine(m_skeletonAnimation, spinePath, "idle", loop: true);
					}
				}
				((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
				UpdateTrial();
				((Component)this).gameObject.SetActive(true);
			}
			else
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public ESkillType GetSkillType()
	{
		return zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(curHeroTid)?.SkillType ?? ESkillType.EmptyValue;
	}

	private void UpdateTrial()
	{
		if (zbServerData.sGameData?.HeroTrialInfo?.CurrentType == GetSkillType() && zbSelf.Instance.trialHandler.HasRedPoint())
		{
			((Component)m_skeletonTrial).gameObject.SetActive(true);
		}
		else
		{
			((Component)m_skeletonTrial).gameObject.SetActive(false);
		}
	}

	public zbRoomHero()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortMainLayerType.Worker;
		vector2 = new Vector2(0.7f, 1.2f);
		((MonoBehaviour)this)._002Ector();
	}
}
