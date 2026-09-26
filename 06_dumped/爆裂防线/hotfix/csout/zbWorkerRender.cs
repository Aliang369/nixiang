using System.Collections;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbWorkerRender : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private Coroutine m_walkCoroutine;

	protected void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("workerAnimation")).GetComponent<SkeletonAnimation>();
	}

	public void OnDisable()
	{
		if (m_walkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_walkCoroutine);
			m_walkCoroutine = null;
		}
	}

	public void Refresh(WorkerStyleInfo styleInfo)
	{
		((Component)this).gameObject.SetActive(true);
		zbStyleInfoHelper.RefreshStyleInfo(m_skeletonAnimation, styleInfo);
		if (m_walkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_walkCoroutine);
			m_walkCoroutine = null;
		}
		m_walkCoroutine = ((MonoBehaviour)this).StartCoroutine(StateLoop());
	}

	private void SetFacing(bool left)
	{
		if (left)
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
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	private Vector3 GetRandomWalkablePos()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = Rand.Range(-2.2f, 2.2f);
		float y = ((Component)m_skeletonAnimation).transform.localPosition.y;
		return new Vector3(num, y, 0f);
	}

	private IEnumerator StateLoop()
	{
		while (true)
		{
			Vector3 localTarget = GetRandomWalkablePos();
			SetAnimation("walk", loop: true);
			while (Vector2.Distance(Vector2.op_Implicit(((Component)m_skeletonAnimation).transform.localPosition), Vector2.op_Implicit(localTarget)) > 0.1f)
			{
				bool facing = ((Component)m_skeletonAnimation).transform.localPosition.x > localTarget.x;
				SetFacing(facing);
				((Component)m_skeletonAnimation).transform.localPosition = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)m_skeletonAnimation).transform.localPosition), Vector2.op_Implicit(localTarget), zbRoomDefine.Instance.walkSpeed * Time.deltaTime));
				yield return null;
			}
			SetAnimation("idle", loop: true);
			yield return (object)new WaitForSeconds(Rand.Range(1.5f, 3.5f));
		}
	}
}
