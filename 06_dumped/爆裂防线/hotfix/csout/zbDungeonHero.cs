using System;
using Proto.Types;
using UnityEngine;

public class zbDungeonHero : MonoBehaviour
{
	[NonSerialized]
	public zbDungeonHeroStateMachine fsm;

	private Transform m_root;

	[NonSerialized]
	public zbDungeonRoom currentRoom;

	[NonSerialized]
	public Vector3 targetPosition;

	[NonSerialized]
	public zbDungeonRoom targetRoom;

	[NonSerialized]
	public DungeonMapGridDetail curDungeonMapGridDetail;

	[NonSerialized]
	public float moveSpeed;

	[NonSerialized]
	public Vector3 heroMoveDir;

	[NonSerialized]
	public DungeonRoleSiteInfo dungeonRoleSiteInfo;

	private zbPlayerAvatar m_playerAvatar;

	private bool m_curAnimLoop;

	private string m_curAnimName;

	public void Init()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		m_root = ((Component)this).transform.Find("root");
		m_playerAvatar = ((Component)m_root.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			((Component)m_playerAvatar).transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
			m_playerAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		}
		fsm = new zbDungeonHeroStateMachine();
		fsm.Init(this);
		fsm.SetState(EDungeonHeroState.Show);
	}

	public void ResetRoleInfo()
	{
		dungeonRoleSiteInfo = null;
		fsm.SetState(EDungeonHeroState.Idle);
	}

	public void SetHeroVisible(bool visible)
	{
		((Component)m_root).gameObject.SetActive(visible);
	}

	public void Refresh()
	{
		RefreshRoleSiteInfo();
	}

	private void RefreshRoleSiteInfo()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		zbDungeonHandler dungeonHandler = zbDungeonMgr.Instance.dungeonHandler;
		DungeonRoleSiteInfo roleSiteInfo = dungeonHandler.GetRoleSiteInfo();
		if (dungeonRoleSiteInfo == null)
		{
			dungeonRoleSiteInfo = roleSiteInfo;
			zbDungeonRoom roomByCellId = zbDungeonMgr.Instance.GetRoomByCellId(dungeonRoleSiteInfo.CellId);
			Vector3 rolePosition = dungeonHandler.GetRolePosition(dungeonRoleSiteInfo, roomByCellId);
			rolePosition.y += 0.08f;
			((Component)this).transform.position = rolePosition;
			SetCurrentRoom(roomByCellId);
			SetFacing(heroMoveDir == Vector3.left);
		}
		else if (roleSiteInfo.CellId != dungeonRoleSiteInfo.CellId)
		{
			zbDungeonRoom roomByCellId2 = zbDungeonMgr.Instance.GetRoomByCellId(roleSiteInfo.CellId);
			if ((Object)(object)roomByCellId2 != (Object)null)
			{
				Vector3 newRoomTargetPosition = dungeonHandler.GetNewRoomTargetPosition(currentRoom, roomByCellId2, roleSiteInfo);
				SetTargetRoomAndPosition(roomByCellId2, newRoomTargetPosition);
				return;
			}
		}
		if (fsm.GetStateId() == EDungeonHeroState.Unknown)
		{
			fsm.SetState(EDungeonHeroState.Idle);
		}
	}

	public void SetCurrentRoom(zbDungeonRoom newRoom)
	{
		zbDungeonHandler dungeonHandler = zbDungeonMgr.Instance.dungeonHandler;
		dungeonHandler.SaveNewRoleSiteInfo(currentRoom, newRoom, dungeonRoleSiteInfo);
		currentRoom = newRoom;
		dungeonHandler.SetRoleSiteInfo_Req(dungeonRoleSiteInfo);
	}

	public void SetTargetRoomAndPosition(zbDungeonRoom newRoom, Vector3 newPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		newPosition.y += 0.08f;
		targetRoom = newRoom;
		targetPosition = newPosition;
		targetPosition.z = ((Component)this).transform.position.z;
		fsm.SetState(EDungeonHeroState.MoveRoom);
	}

	public void OnTriggerEvent()
	{
		fsm.SetState(EDungeonHeroState.EventTrigger);
	}

	public void SetFacing(bool left)
	{
		if (left)
		{
			FaceLeft();
		}
		else
		{
			FaceRight();
		}
	}

	private void FaceLeft()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		m_root.localScale = new Vector3(1f, 1f, 1f);
	}

	private void FaceRight()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		m_root.localScale = new Vector3(-1f, 1f, 1f);
	}

	public void SetAnimation(string animationName, bool loop)
	{
		if (!(m_curAnimName == animationName) || m_curAnimLoop != loop)
		{
			m_curAnimName = animationName;
			m_curAnimLoop = loop;
			m_playerAvatar.SetAnimation(0, animationName, loop);
		}
	}

	private void Update()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		fsm.Update();
		fsm.ProcessMoveInput(zbDungeonMgr.Instance.dungeonWnd.joyStick.GetJoystickInput());
	}

	public zbDungeonHero()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		targetPosition = Vector3.zero;
		moveSpeed = 2f;
		heroMoveDir = Vector3.zero;
		m_curAnimName = string.Empty;
		((MonoBehaviour)this)._002Ector();
	}
}
