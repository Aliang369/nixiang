using System;
using Spine.Unity;
using UnityEngine;

public class zbMergeLeft : MonoBehaviour
{
	[NonSerialized]
	public zbFloor floor;

	private SkeletonAnimation m_spineMergeLeft;

	private BoxCollider2D m_boxCollider;

	private int m_roomLeftId;

	private int m_roomRightId;

	private int m_x;

	private int m_y;

	private void Awake()
	{
		floor = ((Component)((Component)this).transform.parent).GetComponent<zbFloor>();
		m_spineMergeLeft = ((Component)((Component)this).transform).GetComponent<SkeletonAnimation>();
		m_boxCollider = ((Component)((Component)this).transform).GetComponent<BoxCollider2D>();
	}

	public void Build()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floor == (Object)null)
		{
			floor = ((Component)((Component)this).transform.parent).GetComponent<zbFloor>();
			if ((Object)(object)floor == (Object)null)
			{
				return;
			}
		}
		bool active = false;
		zbRoom roomAt = floor.GetRoomAt(1);
		zbRoom roomAt2 = floor.GetRoomAt(2);
		zbRoom roomAt3 = floor.GetRoomAt(3);
		Vector3 position = ((Component)m_spineMergeLeft).transform.position;
		int val = 2;
		if ((Object)(object)roomAt != (Object)null && (Object)(object)roomAt2 != (Object)null && roomAt.CanMerge() && roomAt2.CanMerge() && roomAt.roomRecord.type == roomAt2.roomRecord.type && roomAt.roomRecord.subType == roomAt2.roomRecord.subType)
		{
			if (roomAt.roomRecord.length == 1 && roomAt2.roomRecord.length == 1)
			{
				m_spineMergeLeft.AnimationState.SetAnimation(0, "merge11", true);
				position.x = 0.1f;
				active = true;
				m_roomLeftId = roomAt.roomData.RoomId;
				m_roomRightId = roomAt2.roomData.RoomId;
				m_x = roomAt.roomData.X;
				m_y = roomAt.roomData.Y;
			}
			else if (roomAt.roomRecord.length == 1 && roomAt2.roomRecord.length == 2)
			{
				m_spineMergeLeft.AnimationState.SetAnimation(0, "merge12", true);
				position.x = 1.7f;
				active = true;
				m_roomLeftId = roomAt.roomData.RoomId;
				m_roomRightId = roomAt2.roomData.RoomId;
				m_x = roomAt.roomData.X;
				m_y = roomAt.roomData.Y;
				val = 3;
			}
		}
		else if ((Object)(object)roomAt != (Object)null && (Object)(object)roomAt3 != (Object)null && roomAt.CanMerge() && roomAt3.CanMerge() && roomAt.roomRecord.type == roomAt3.roomRecord.type && roomAt.roomRecord.subType == roomAt3.roomRecord.subType)
		{
			if (roomAt.roomRecord.length == 2 && roomAt3.roomRecord.length == 1)
			{
				m_spineMergeLeft.AnimationState.SetAnimation(0, "merge21", true);
				position.x = 1.7f;
				active = true;
				m_roomLeftId = roomAt.roomData.RoomId;
				m_roomRightId = roomAt3.roomData.RoomId;
				m_x = roomAt.roomData.X;
				m_y = roomAt.roomData.Y;
				val = 3;
			}
		}
		else if ((Object)(object)roomAt2 != (Object)null && (Object)(object)roomAt3 != (Object)null && roomAt2.CanMerge() && roomAt3.CanMerge() && roomAt2.roomRecord.type == roomAt3.roomRecord.type && roomAt2.roomRecord.subType == roomAt3.roomRecord.subType && roomAt2.roomRecord.length == 1 && roomAt3.roomRecord.length == 1)
		{
			m_spineMergeLeft.AnimationState.SetAnimation(0, "merge11", true);
			position.x = 3.3f;
			active = true;
			m_roomLeftId = roomAt2.roomData.RoomId;
			m_roomRightId = roomAt3.roomData.RoomId;
			m_x = roomAt2.roomData.X;
			m_y = roomAt2.roomData.Y;
		}
		((Component)m_spineMergeLeft).gameObject.SetActive(active);
		((Component)m_spineMergeLeft).transform.position = position;
		int num = Math.Max(val, 1);
		m_boxCollider.size = new Vector2((float)num * 3f, 1.8f);
	}

	public void Refresh(zbRoom roomLeft, zbRoom roomRight)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		int num = roomLeft.roomRecord.length + roomRight.roomRecord.length;
		Vector3 position = ((Component)m_spineMergeLeft).transform.position;
		m_spineMergeLeft.AnimationState.SetAnimation(0, $"merge{roomLeft.roomRecord.length}{roomRight.roomRecord.length}", true);
		m_roomLeftId = roomLeft.roomData.RoomId;
		m_roomRightId = roomRight.roomData.RoomId;
		m_x = roomLeft.roomData.X;
		m_y = roomLeft.roomData.Y;
		position.x = zbRoomDefine.Instance.GetRoomPostionX(num, roomLeft.roomData.X) + 7.6f;
		((Component)m_spineMergeLeft).transform.position = position;
		m_boxCollider.size = new Vector2((float)num * 3f + (float)(num - 1) * 0.2f, 1.8f);
	}

	public RoomComposeData GetRoomComposeData()
	{
		return new RoomComposeData(m_roomLeftId, m_roomRightId, m_x, m_y);
	}
}
