using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbRoomCreate : MonoBehaviour
{
	[NonSerialized]
	public zbFloor floor;

	[NonSerialized]
	public int x;

	[NonSerialized]
	public int y;

	private BoxCollider2D m_boxCollider;

	private SkeletonAnimation m_spineState;

	private zbHomeRoomTypeCsvRecord m_roomRecord;

	private void Awake()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		zbHomeMgr.Instance.roomMgr.roomCreateList.Add(this);
		m_boxCollider = ((Component)((Component)this).transform.Find("roomCreate")).GetComponent<BoxCollider2D>();
		m_spineState = ((Component)((Component)this).transform.Find("spineState")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineState).updateWhenInvisible = (UpdateMode)0;
	}

	private void OnDestroy()
	{
	}

	public void Refresh(zbRoomCreateData roomCreateData)
	{
		((Component)this).gameObject.SetActive(zbSelf.Instance.homelandHandler.GetFloorState(roomCreateData.y) == EActiveState.Done);
		x = roomCreateData.x;
		y = roomCreateData.y;
		m_roomRecord = roomCreateData.record;
		RefreshPostion();
		RefreshRoomState();
		RefreshRoomWidth();
	}

	private void RefreshPostion()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(zbRoomDefine.Instance.GetRoomPostionX(m_roomRecord.length, x), 0f, 0f);
		((Component)this).transform.localScale = Vector3.one;
	}

	private void RefreshRoomState()
	{
		m_spineState.AnimationState.SetAnimation(0, "build" + m_roomRecord.length, true);
	}

	private void RefreshRoomWidth()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Max(m_roomRecord.length, 1);
		m_boxCollider.size = new Vector2((float)num * 3f, 1.8f);
	}
}
