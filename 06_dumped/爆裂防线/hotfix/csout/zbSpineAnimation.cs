using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSpineAnimation
{
	private MonoBehaviour m_mono;

	public SkeletonAnimation skeletonAnimation;

	public GameObject skeletonAnimationGO;

	private bool m_isSkeleActive = true;

	public zbSpineGpuAnimPlayer gpuPlayer;

	private bool GpuActive
	{
		get
		{
			if (gpuPlayer != null)
			{
				return !gpuPlayer.isFallback;
			}
			return false;
		}
	}

	public void Init(MonoBehaviour owner)
	{
		m_mono = owner;
		skeletonAnimation = ((Component)m_mono).GetComponentInChildren<SkeletonAnimation>();
		skeletonAnimationGO = ((Component)skeletonAnimation).gameObject;
		m_isSkeleActive = true;
	}

	public void Free()
	{
	}

	public void PlayAnimation(string animationName, bool loop)
	{
		if (m_isSkeleActive && !((Object)(object)skeletonAnimation == (Object)null) && ((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animationName) != null)
		{
			skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
			if (GpuActive)
			{
				gpuPlayer.PlayClip(animationName, loop);
			}
		}
	}

	public void AddAnimation(string animationName, bool loop)
	{
		if (m_isSkeleActive && !((Object)(object)skeletonAnimation == (Object)null) && ((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animationName) != null)
		{
			skeletonAnimation.AnimationState.AddAnimation(0, animationName, loop, 0f);
			if (GpuActive)
			{
				gpuPlayer.QueueClip(animationName, loop);
			}
		}
	}

	public void StopAnimation(int tackIndex)
	{
		if (!((Object)(object)skeletonAnimation != (Object)null))
		{
			return;
		}
		TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(tackIndex);
		if (current == null || current.Animation.Name != "<empty>")
		{
			skeletonAnimation.AnimationState.SetEmptyAnimation(tackIndex, 0f);
			if (GpuActive && tackIndex == 0)
			{
				gpuPlayer.PlaySetupPose();
			}
		}
	}

	public TrackEntry PlayTackAnimation(int tackIndex, string animationName, bool loop)
	{
		if (!m_isSkeleActive || (Object)(object)skeletonAnimation == (Object)null || ((SkeletonRenderer)skeletonAnimation).skeleton.Data.FindAnimation(animationName) == null)
		{
			return null;
		}
		if (GpuActive && tackIndex > 0)
		{
			zbSpineGpuAnimRuntime.NotifyRuntimeFallback();
			gpuPlayer.FallbackToSpine();
		}
		TrackEntry result = skeletonAnimation.AnimationState.SetAnimation(tackIndex, animationName, loop);
		if (GpuActive && tackIndex == 0)
		{
			gpuPlayer.PlayClip(animationName, loop);
		}
		return result;
	}

	public void SetTimeScale(float scale)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			skeletonAnimation.timeScale = scale;
		}
		if (GpuActive)
		{
			gpuPlayer.timeScale = scale;
		}
	}

	public void RandomizeCurrentTrackTime()
	{
		if (!((Object)(object)skeletonAnimation == (Object)null))
		{
			TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(0);
			if (current != null)
			{
				current.TrackTime = Rand.Range(0f, current.AnimationEnd);
			}
			skeletonAnimation.Update(0f);
			if (GpuActive)
			{
				gpuPlayer.RandomizeTime();
			}
		}
	}

	public TrackEntry GetCurrentTrackEntry(int tackIndex)
	{
		if (!((Object)(object)skeletonAnimation != (Object)null))
		{
			return null;
		}
		return skeletonAnimation.AnimationState.GetCurrent(tackIndex);
	}

	public void ResetAnimations()
	{
		StopAnimation(0);
		StopAnimation(1);
		SetTimeScale(1f);
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			skeletonAnimation.AnimationState.SetEmptyAnimation(0, 0f);
			skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
		}
		if (gpuPlayer != null)
		{
			gpuPlayer.Reset();
		}
	}

	public Bone FindBone(string name)
	{
		if (Object.op_Implicit((Object)(object)skeletonAnimation))
		{
			return ((SkeletonRenderer)skeletonAnimation).skeleton.FindBone(name);
		}
		return null;
	}

	public void RefreshSpine(string spinePath)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			SkeletonDataAsset val = zbCombatAtlasMerger.Instance.GetMerged(spinePath);
			if ((Object)(object)val == (Object)null)
			{
				val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
			}
			if ((Object)(object)val != (Object)null && (!((Object)(object)((SkeletonRenderer)skeletonAnimation).skeletonDataAsset == (Object)(object)val) || ((SkeletonRenderer)skeletonAnimation).skeleton == null))
			{
				((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = val;
				((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
			}
		}
	}

	public void SetActive(bool bValue)
	{
		if (Object.op_Implicit((Object)(object)skeletonAnimation))
		{
			skeletonAnimationGO.SetActive(bValue);
			m_isSkeleActive = bValue;
		}
	}
}
