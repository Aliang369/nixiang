using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbRoomEmpty : MonoBehaviour
{
	public enum EBuildStep
	{
		None,
		In,
		Loop,
		Out,
		Complete
	}

	[NonSerialized]
	public zbFloor floor;

	[NonSerialized]
	public int x;

	[NonSerialized]
	public int y;

	[NonSerialized]
	public SpriteRenderer m_spriteRenderer;

	[NonSerialized]
	public SkeletonAnimation m_spineState;

	private BoxCollider2D m_boxCollider;

	private Coroutine m_roomCoroutine;

	private EBuildStep m_currentStep;

	protected ESortMainLayerType m_sortType = ESortMainLayerType.RoomBg;

	private void Awake()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		zbHomeMgr.Instance.roomMgr.roomEmptyList.Add(this);
		m_spriteRenderer = ((Component)((Component)this).transform.Find("roomEmpty")).GetComponent<SpriteRenderer>();
		m_boxCollider = ((Component)((Component)this).transform.Find("roomEmpty")).GetComponent<BoxCollider2D>();
		m_spineState = ((Component)((Component)this).transform.Find("spineState")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineState).updateWhenInvisible = (UpdateMode)0;
		((Component)m_spineState).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		if (m_currentStep != EBuildStep.Complete && m_currentStep != EBuildStep.None)
		{
			PlayerBuildingAnimation(resume: true);
		}
	}

	public void Refresh(int _x, int _y)
	{
		((Component)this).gameObject.SetActive(zbSelf.Instance.homelandHandler.GetFloorState(_y) == EActiveState.Done);
		x = _x;
		y = _y;
		floor = ((Component)((Component)this).transform.parent).GetComponent<zbFloor>();
		RefreshPosition();
	}

	private void RefreshPosition()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(zbRoomDefine.Instance.GetRoomPostionX(1, x), 0f, 0f);
		((Component)this).transform.localScale = Vector3.one;
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void PlayerBuildingAnimation(bool resume = false)
	{
		if (m_roomCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_roomCoroutine);
		}
		m_roomCoroutine = ((MonoBehaviour)this).StartCoroutine(BuildingCoroutine(resume));
	}

	private IEnumerator BuildingCoroutine(bool resume)
	{
		((Component)m_spriteRenderer).gameObject.SetActive(false);
		((Component)m_spineState).gameObject.SetActive(true);
		if (!resume || m_currentStep == EBuildStep.None)
		{
			m_currentStep = EBuildStep.In;
			yield return PlayAnimationAndWait("building1_in", loop: false);
		}
		if (!resume || m_currentStep == EBuildStep.In)
		{
			m_currentStep = EBuildStep.Loop;
			yield return PlayAnimationAndWait("building1_loop", loop: false);
		}
		if (!resume || m_currentStep == EBuildStep.Loop)
		{
			m_currentStep = EBuildStep.Out;
			yield return PlayAnimationAndWait("building1_out", loop: false);
		}
		((Component)m_spriteRenderer).gameObject.SetActive(true);
		((Component)m_spineState).gameObject.SetActive(false);
		m_currentStep = EBuildStep.Complete;
		m_roomCoroutine = null;
	}

	private IEnumerator PlayAnimationAndWait(string animationName, bool loop)
	{
		TrackEntry val = m_spineState.AnimationState.SetAnimation(0, animationName, loop);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
	}
}
