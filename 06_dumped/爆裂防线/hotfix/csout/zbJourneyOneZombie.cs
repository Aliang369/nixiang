using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbJourneyOneZombie : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private Coroutine m_stateCoroutine;

	private int m_avatarId;

	private Vector3 m_startPos;

	private Vector3 m_endPos;

	private bool m_isWalking;

	protected void Awake()
	{
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
	}

	protected void OnEnable()
	{
		if (m_avatarId > 0 && !m_isWalking)
		{
			StartWalk();
		}
	}

	protected void OnDisable()
	{
		StopWalk();
	}

	public void Refresh(int avatarId, Vector3 startPos)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		m_avatarId = avatarId;
		m_startPos = startPos;
		if (startPos.x < 0f)
		{
			m_endPos = startPos + new Vector3(8f, 0f, 0f);
		}
		else
		{
			m_endPos = startPos + new Vector3(-8f, 0f, 0f);
		}
		zbNpcAvatarCsvRecord recordByKey = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			zbStyleInfoHelper.RefreshNpcAvatar(m_skeletonAnimation, recordByKey);
		}
		((Component)this).transform.position = m_startPos;
		SetFacing(m_endPos.x < m_startPos.x);
		StartWalk();
	}

	private void StartWalk()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			StopWalk();
			m_isWalking = true;
			m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(WalkToEnd());
		}
	}

	private void StopWalk()
	{
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_isWalking = false;
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

	private IEnumerator WalkToEnd()
	{
		SetAnimation("walk_zombie", loop: true);
		while (Vector3.Distance(((Component)this).transform.position, m_endPos) > 0.1f)
		{
			SetFacing(((Component)this).transform.position.x > m_endPos.x);
			((Component)this).transform.position = Vector3.MoveTowards(((Component)this).transform.position, m_endPos, zbRoomDefine.Instance.walkZombieSpeed * Time.deltaTime);
			yield return null;
		}
	}
}
