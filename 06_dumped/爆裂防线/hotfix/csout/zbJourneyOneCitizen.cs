using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbJourneyOneCitizen : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private Coroutine m_stateCoroutine;

	private Vector3 m_initPos;

	private bool m_isActive;

	private bool m_hasTarget;

	private Vector3 m_targetPos;

	protected void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
	}

	protected void OnEnable()
	{
		if (m_isActive)
		{
			StartIdleWalk();
		}
	}

	protected void OnDisable()
	{
		StopIdleWalk();
	}

	public void Refresh(int avatarId, Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_initPos = pos;
		m_isActive = true;
		zbNpcAvatarCsvRecord recordByKey = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			zbStyleInfoHelper.RefreshNpcAvatar(m_skeletonAnimation, recordByKey);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(1, "mount_gunbag", true);
		((Component)this).transform.position = pos;
		SetFacing(left: false);
		StartIdleWalk();
	}

	public void Refresh(int avatarId, Vector3 pos, Vector3 targetPos)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		m_hasTarget = true;
		m_initPos = pos;
		m_targetPos = targetPos;
		m_isActive = true;
		zbNpcAvatarCsvRecord recordByKey = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			zbStyleInfoHelper.RefreshNpcAvatar(m_skeletonAnimation, recordByKey);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(1, "mount_gunbag", true);
		((Component)this).transform.position = pos;
		SetFacing(left: false);
		StartIdleWalk();
	}

	private void StartIdleWalk()
	{
		StopIdleWalk();
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(IdleWalkLoop());
	}

	private void StopIdleWalk()
	{
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	public void SetFacing(bool left)
	{
		((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = (left ? 1 : (-1));
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	private IEnumerator IdleWalkLoop()
	{
		while (true)
		{
			Vector3 targetPos = ((!m_hasTarget) ? GetRandomWalkablePos() : ((Vector3.Distance(((Component)this).transform.position, m_initPos) < 0.1f) ? m_targetPos : m_initPos));
			SetAnimation("walk", loop: true);
			targetPos.z = ((Component)this).transform.position.z;
			while (Vector3.Distance(((Component)this).transform.position, targetPos) > 0.1f)
			{
				SetFacing(((Component)this).transform.position.x > targetPos.x);
				((Component)this).transform.position = Vector3.MoveTowards(((Component)this).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
				yield return null;
			}
			SetAnimation("idle", loop: true);
			if (m_hasTarget)
			{
				yield return (object)new WaitForSeconds(Rand.Range(1f, 3f));
			}
			else
			{
				yield return (object)new WaitForSeconds(Rand.Range(5f, 10f));
			}
		}
	}

	public Vector3 GetRandomWalkablePos()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float min = m_initPos.x - 0.4f;
		float max = m_initPos.x + 0.4f;
		return new Vector3(Rand.Range(min, max), m_initPos.y, -1f);
	}
}
