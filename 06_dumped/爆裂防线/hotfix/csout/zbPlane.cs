using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbPlane : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private float m_WalkPosGapY = 0.3f;

	private Coroutine m_planeCoroutine;

	private void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform).GetComponent<SkeletonAnimation>();
	}

	public IEnumerator OpenDoor()
	{
		if ((Object)(object)m_skeletonAnimation != (Object)null && (Object)(object)((Component)m_skeletonAnimation).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_skeletonAnimation, "opendoor"))
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "opendoor", false);
		}
		yield return (object)new WaitForSeconds(0.1f);
	}

	public IEnumerator CloseDoor(float delayTime = 0f)
	{
		if ((Object)(object)m_skeletonAnimation != (Object)null && (Object)(object)((Component)m_skeletonAnimation).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_skeletonAnimation, "closedoor"))
		{
			TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "closedoor", false);
			if (val != null && delayTime > 0f)
			{
				val.Delay = delayTime;
			}
		}
		yield return null;
	}

	public Vector3 GetCenterPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position - new Vector3(0f, 0.15f, 0f);
	}

	public Vector3 GetInsidePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f, m_WalkPosGapY, 0f);
	}

	public void PlayerArriveAnimation(float duration)
	{
		if (m_planeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_planeCoroutine);
		}
		m_planeCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayArriveCoroutine(duration));
	}

	private IEnumerator PlayArriveCoroutine(float duration)
	{
		m_skeletonAnimation.AnimationState.SetAnimation(0, "fly_move", false);
		yield return (object)new WaitForSeconds(2f);
		yield return OpenDoor();
		yield return CloseDoor(duration);
		m_planeCoroutine = null;
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		return m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
	}
}
