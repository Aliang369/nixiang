using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbEndlessMineHero : MonoBehaviour
{
	private SkeletonAnimation m_skeletonAnimation;

	private Coroutine m_stateCoroutine;

	private Vector3 m_initPos;

	private bool m_isActive;

	private float m_offsetX = 0.4f;

	private float m_offsetY;

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

	public void Refresh(int heroTid, Vector3 pos, float offsetX = 0f, float offsetY = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		m_initPos = pos;
		m_offsetX = offsetX;
		m_offsetY = offsetY;
		m_isActive = true;
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroTid);
		if (recordByKey != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				string spinePath = recordByKey2.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_skeletonAnimation, spinePath, "idle", loop: true);
			}
		}
		((Component)this).transform.position = GetRandomWalkablePos();
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
			Vector3 targetPos = GetRandomWalkablePos();
			SetAnimation("walk", loop: true);
			targetPos.z = ((Component)this).transform.position.z;
			while (Vector3.Distance(((Component)this).transform.position, targetPos) > 0.1f)
			{
				SetFacing(((Component)this).transform.position.x > targetPos.x);
				((Component)this).transform.position = Vector3.MoveTowards(((Component)this).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
				yield return null;
			}
			SetAnimation("idle", loop: true);
			yield return (object)new WaitForSeconds(Rand.Range(2f, 5f));
		}
	}

	public Vector3 GetRandomWalkablePos()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float min = m_initPos.x - m_offsetX;
		float max = m_initPos.x + m_offsetX;
		float min2 = m_initPos.y - m_offsetY;
		float max2 = m_initPos.y + m_offsetY;
		float num = Rand.Range(min, max);
		float num2 = Rand.Range(min2, max2);
		return new Vector3(num, num2, m_initPos.z);
	}
}
