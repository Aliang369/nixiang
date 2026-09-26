using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbWallAvatar : MonoBehaviour
{
	[NonSerialized]
	public SkeletonAnimation m_spineAvatarBody;

	private GameObject skeletonObject;

	private SkeletonAnimation m_spineGm;

	[NonSerialized]
	public GameObject gmObject;

	private bool isInitialized;

	private void InitializeAvatar()
	{
		if (!isInitialized)
		{
			skeletonObject = ((Component)((Component)this).transform.Find("wallAnimation")).gameObject;
			m_spineAvatarBody = skeletonObject.GetComponent<SkeletonAnimation>();
			gmObject = ((Component)((Component)this).transform.Find("wallAnimationBm")).gameObject;
			m_spineGm = gmObject.GetComponent<SkeletonAnimation>();
			isInitialized = true;
		}
	}

	public void RefreshCustom(int wallId)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		InitializeAvatar();
		if (wallId != 0)
		{
			zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineAvatarBody, recordByKey.animation + "_SkeletonData.asset", "1", loop: true);
				if ((Object)(object)zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(recordByKey.animation + "_bm_SkeletonData.asset") != (Object)null)
				{
					zbUtil.RefreshSpine(m_spineGm, recordByKey.animation + "_bm_SkeletonData.asset", "1", loop: true);
					gmObject.SetActive(true);
				}
				else
				{
					gmObject.SetActive(false);
				}
			}
		}
		((SkeletonRenderer)m_spineAvatarBody).updateWhenInvisible = (UpdateMode)0;
	}

	public void RefreshSpine(string spinePath, string animaiton = "", bool loop = false)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		InitializeAvatar();
		zbUtil.RefreshSpine(m_spineAvatarBody, spinePath + "_SkeletonData.asset", "1", loop: true);
		if ((Object)(object)zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath + "_bm_SkeletonData.asset") != (Object)null)
		{
			zbUtil.RefreshSpine(m_spineGm, spinePath + "_bm_SkeletonData.asset", "1", loop: true);
			gmObject.SetActive(true);
		}
		else
		{
			gmObject.SetActive(false);
		}
		((SkeletonRenderer)m_spineAvatarBody).updateWhenInvisible = (UpdateMode)0;
	}

	public void SetSuperWallInAnimation(bool isFirst = false)
	{
		if (isFirst && zbUtil.CheckAnimationNameExist(m_spineAvatarBody, "in"))
		{
			Skeleton skeleton = ((SkeletonRenderer)m_spineAvatarBody).Skeleton;
			AnimationState animationState = m_spineAvatarBody.AnimationState;
			animationState.ClearTracks();
			skeleton.SetToSetupPose();
			animationState.Apply(skeleton);
			if (gmObject.activeSelf)
			{
				_ = ((SkeletonRenderer)m_spineGm).Skeleton;
				_ = m_spineGm.AnimationState;
				animationState.ClearTracks();
				skeleton.SetToSetupPose();
				animationState.Apply(skeleton);
				m_spineGm.AnimationState.SetAnimation(0, "in", false);
				m_spineGm.AnimationState.AddAnimation(0, "1", true, 0f);
			}
			m_spineAvatarBody.AnimationState.SetAnimation(0, "in", false);
			m_spineAvatarBody.AnimationState.AddAnimation(0, "1", true, 0f);
		}
	}

	public TrackEntry SetAnimation(int trackIndex, string animationName, bool loop)
	{
		if ((Object)(object)m_spineAvatarBody == (Object)null)
		{
			return null;
		}
		TrackEntry current = m_spineAvatarBody.AnimationState.GetCurrent(trackIndex);
		if (current != null && current.Animation.Name == animationName)
		{
			return current;
		}
		TrackEntry result = m_spineAvatarBody.AnimationState.SetAnimation(trackIndex, animationName, loop);
		m_spineAvatarBody.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		if (gmObject.activeSelf)
		{
			m_spineGm.AnimationState.SetAnimation(trackIndex, animationName, loop);
			m_spineGm.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		}
		return result;
	}
}
