using System;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbLeaver : MonoBehaviour
{
	[NonSerialized]
	public zbLeaverStateMachine m_fsm;

	private SkeletonAnimation m_skeletonAnimation;

	[NonSerialized]
	public ESortMainLayerType m_sortType = ESortMainLayerType.Worker;

	[NonSerialized]
	public WorkerDetail m_homelandWorkerDetail;

	private WorkerStyleInfo m_initStyleInfo;

	protected void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		m_fsm = new zbLeaverStateMachine();
		m_fsm.Init(this);
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	protected void OnEnable()
	{
		m_fsm.OnEnable();
	}

	protected void Update()
	{
		m_fsm.Update();
		UpdateZorder();
	}

	private void UpdateZorder()
	{
		zbSortMainMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void SetFacing(bool left)
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

	public void Init(WorkerDetail workerDetail, WorkerStyleInfo initStyleInfo, Vector3 position, bool faceLeft)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		m_homelandWorkerDetail = workerDetail;
		m_initStyleInfo = initStyleInfo;
		((Component)this).transform.position = position;
		SetFacing(faceLeft);
		zbStyleInfoHelper.RefreshStyleInfo(m_skeletonAnimation, workerDetail.StyleInfo);
		if (workerDetail.StyleInfo.ToString() != m_initStyleInfo.ToString())
		{
			m_fsm.SetState(ELeaverState.ChangeClose);
		}
		else
		{
			m_fsm.SetState(ELeaverState.MoveToExit);
		}
	}

	public void RefreshInitSuit()
	{
		zbStyleInfoHelper.RefreshStyleInfo(m_skeletonAnimation, m_initStyleInfo);
	}
}
