using Proto.Enums;
using UnityEngine;

public class zbDungeonHeroElevatorState : zbDungeonHeroState
{
	private Vector3 progressPosition;

	private zbHeroActionPerformer actionPerformer;

	private Vector3 m_startDir;

	public zbDungeonHeroElevatorState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		progressPosition = Vector3.zero;
		base._002Ector(fsm, eState);
		actionPerformer = new zbHeroActionPerformer((MonoBehaviour)(object)m_fsm.hero);
	}

	public override void OnEnter()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		m_fsm.hero.SetAnimation("idle", loop: true);
		m_startDir = m_fsm.hero.heroMoveDir;
		OnElevatorAction();
	}

	public override void OnExit()
	{
		base.OnExit();
		StopAction();
	}

	private void StopAction()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		actionPerformer.Stop();
		zbDungeonMgr.Instance.dungeonWnd.SetActionProgress(actionPerformer.IsRunning, actionPerformer.Progress, actionPerformer.ProgressPosition);
	}

	public override void ProcessMoveInput(Vector2 joystickInput)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMoveInput(joystickInput);
		if (Vector2.op_Implicit(joystickInput) != m_startDir)
		{
			StopAction();
			m_fsm.SetState(EDungeonHeroState.Idle);
		}
	}

	private void OnElevatorAction()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		progressPosition = new Vector3(((Component)m_fsm.hero).transform.position.x, ((Component)m_fsm.hero).transform.position.y + 1.5f, progressPosition.z);
		actionPerformer.StartAction(actionTime, progressPosition, OnActionEnd);
	}

	private void OnActionEnd()
	{
		m_fsm.hero.SetAnimation("idle", loop: true);
		zbDungeonRoom nextElevatorRoom = zbDungeonMgr.Instance.GetNextElevatorRoom();
		if ((Object)(object)nextElevatorRoom == (Object)null)
		{
			m_fsm.SetState(EDungeonHeroState.Idle);
		}
		else
		{
			ElevatorCheck(nextElevatorRoom);
		}
	}

	private void ElevatorCheck(zbDungeonRoom nextRoom)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (nextRoom.dungeonMapCellDetail.State == EDungeonMapCellState.Invisible)
		{
			m_fsm.SetState(EDungeonHeroState.Idle);
		}
		else if (nextRoom.dungeonMapCellDetail.State == EDungeonMapCellState.Visible)
		{
			zbDungeonMgr.Instance.dungeonHandler.OpenCell_Req(m_fsm.hero.currentRoom.dungeonMapCellDetail.CellId, nextRoom.dungeonMapCellDetail.CellId);
		}
		else
		{
			m_fsm.hero.SetTargetRoomAndPosition(nextRoom, nextRoom.GetCenterPos());
		}
	}

	public override void OnUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		zbDungeonMgr.Instance.dungeonWnd.SetActionProgress(actionPerformer.IsRunning, actionPerformer.Progress, actionPerformer.ProgressPosition);
	}
}
