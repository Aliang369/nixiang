using UnityEngine;

public class zbDungeonHeroOpenDoorState : zbDungeonHeroState
{
	private Vector3 progressPosition;

	private zbHeroActionPerformer actionPerformer;

	private Vector3 m_startDir;

	public zbDungeonHeroOpenDoorState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		progressPosition = Vector3.zero;
		base._002Ector(fsm, eState);
		actionPerformer = new zbHeroActionPerformer((MonoBehaviour)(object)m_fsm.hero);
	}

	public override void OnEnter()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		actionTime = 0.5f;
		m_fsm.hero.SetAnimation("action", loop: true);
		m_startDir = m_fsm.hero.heroMoveDir;
		OnOpenDoorAction();
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

	private void OnOpenDoorAction()
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
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.hero.SetAnimation("idle", loop: true);
		if (m_fsm.hero.currentRoom.IsSpawn() && m_fsm.hero.heroMoveDir == Vector3.left)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10318"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbDungeonMgr.Instance.dungeonHandler.CloseDungeon_Req();
				}
			});
		}
		else
		{
			zbDungeonRoom nextRoom = zbDungeonMgr.Instance.GetNextRoom(m_fsm.hero.currentRoom, m_fsm.hero.heroMoveDir == Vector3.left);
			if ((Object)(object)nextRoom == (Object)null)
			{
				m_fsm.SetState(EDungeonHeroState.Idle);
			}
			else
			{
				zbDungeonMgr.Instance.dungeonHandler.OpenCell_Req(m_fsm.hero.currentRoom.dungeonMapCellDetail.CellId, nextRoom.dungeonMapCellDetail.CellId);
			}
		}
	}

	public override void OnUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		zbDungeonMgr.Instance.dungeonWnd.SetActionProgress(actionPerformer.IsRunning, actionPerformer.Progress, actionPerformer.ProgressPosition);
	}
}
