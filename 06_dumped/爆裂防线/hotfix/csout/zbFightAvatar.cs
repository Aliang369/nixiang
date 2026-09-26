using Spine;
using Spine.Unity;
using UnityEngine;

public class zbFightAvatar : MonoBehaviour
{
	private SkeletonAnimation m_spineAvatarBody;

	private GameObject skeletonObject;

	private SkeletonAnimation m_spineGun;

	private GameObject gunObject;

	private bool isInitialized;

	private void Awake()
	{
		InitializeAvatar();
	}

	private void InitializeAvatar()
	{
		if (!isInitialized)
		{
			skeletonObject = ((Component)((Component)this).transform.Find("avatarBody")).gameObject;
			m_spineAvatarBody = skeletonObject.GetComponent<SkeletonAnimation>();
			gunObject = ((Component)((Component)this).transform.Find("gun")).gameObject;
			m_spineGun = gunObject.GetComponent<SkeletonAnimation>();
			isInitialized = true;
		}
	}

	public void RefreshCustom(int avatarId, int gunAvatarId)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		InitializeAvatar();
		if (avatarId != 0)
		{
			zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineAvatarBody, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
			}
		}
		if (gunAvatarId != 0)
		{
			zbPlayerGunCsvRecord recordByKey2 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunAvatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(m_spineGun, recordByKey2.fightBone + "_SkeletonData.asset", "animation", loop: true);
			}
		}
		((SkeletonRenderer)m_spineAvatarBody).updateWhenInvisible = (UpdateMode)0;
	}

	public void RandomAnimation()
	{
		TrackEntry obj = m_spineAvatarBody.state.SetAnimation(0, "idle", true);
		float duration = obj.Animation.Duration;
		float trackTime = Rand.Range(0f, duration);
		obj.TrackTime = trackTime;
	}

	public TrackEntry SetAnimation(int trackIndex, string animationName, bool loop)
	{
		TrackEntry result = m_spineAvatarBody.AnimationState.SetAnimation(trackIndex, animationName, loop);
		m_spineAvatarBody.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	public TrackEntry AddAnimation(int trackIndex, string animationName, bool loop)
	{
		return m_spineAvatarBody.AnimationState.AddAnimation(trackIndex, animationName, loop, 0f);
	}

	public void SetScale(Vector2 vector2)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector2.op_Implicit(vector2);
	}

	public void SetFacing(bool left)
	{
		if (left)
		{
			((SkeletonRenderer)m_spineAvatarBody).Skeleton.ScaleX = 1f;
		}
		else
		{
			((SkeletonRenderer)m_spineAvatarBody).Skeleton.ScaleX = -1f;
		}
	}

	public string GetAnimationName(int trackIndex)
	{
		TrackEntry current = m_spineAvatarBody.AnimationState.GetCurrent(trackIndex);
		if (current != null && current.Animation != null)
		{
			_ = current.Animation.Name;
			return current.Animation.Name;
		}
		return "";
	}
}
