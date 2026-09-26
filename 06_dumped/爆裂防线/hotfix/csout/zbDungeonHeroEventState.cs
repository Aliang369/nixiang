using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonHeroEventState : zbDungeonHeroState
{
	private Transform m_heroTransform;

	private Vector3 progressPosition;

	private zbHeroActionPerformer actionPerformer;

	private DungeonMapCellDetail m_mapCellDetail;

	private DungeonMapGridDetail m_mapGridDetail;

	private Vector2 m_eventPos;

	public zbDungeonHeroEventState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		progressPosition = Vector3.zero;
		m_eventPos = Vector2.zero;
		base._002Ector(fsm, eState);
		actionPerformer = new zbHeroActionPerformer((MonoBehaviour)(object)m_fsm.hero);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.hero.SetAnimation("idle", loop: true);
		m_mapCellDetail = m_fsm.hero.currentRoom.dungeonMapCellDetail;
		m_heroTransform = ((Component)m_fsm.hero).transform;
		m_mapGridDetail = m_fsm.hero.curDungeonMapGridDetail;
		StopAction();
		TriggerEvent();
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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMoveInput(joystickInput);
		Vector3 val = Vector2.op_Implicit(joystickInput);
		if (val == Vector3.left || val == Vector3.right)
		{
			m_fsm.hero.SetFacing(val == Vector3.left);
			m_fsm.hero.SetAnimation("run", loop: true);
			OnMove(val);
		}
		else if (val == Vector3.zero)
		{
			m_fsm.hero.SetAnimation("idle", loop: true);
		}
	}

	private void OnMove(Vector3 movedir)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		_ = m_mapGridDetail;
		float num = m_heroTransform.position.x + movedir.x * m_fsm.hero.moveSpeed * Time.unscaledDeltaTime;
		m_heroTransform.position = new Vector3(num, m_heroTransform.position.y, m_heroTransform.position.z);
		SkeletonAnimation skeletonAnimationInGrid = m_fsm.hero.currentRoom.GetSkeletonAnimationInGrid(m_mapGridDetail.GridId);
		if (!((Object)(object)skeletonAnimationInGrid == (Object)null) && Mathf.Abs(((Component)skeletonAnimationInGrid).transform.position.x - m_heroTransform.position.x) > eventTriggerRange)
		{
			StopAction();
			m_fsm.SetState(EDungeonHeroState.Run);
		}
	}

	private void TriggerEvent()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		m_mapGridDetail = m_fsm.hero.currentRoom.GetRoomGridIDetailFromPositionX(m_heroTransform.position.x);
		SkeletonAnimation skeletonAnimationInGrid = m_fsm.hero.currentRoom.GetSkeletonAnimationInGrid(m_mapGridDetail.GridId);
		if ((Object)(object)skeletonAnimationInGrid != (Object)null)
		{
			m_eventPos = Vector2.op_Implicit(((Component)skeletonAnimationInGrid).transform.position);
		}
		if (m_mapGridDetail != null)
		{
			if (m_mapGridDetail.Trigger == EDungeonEventTrigger.Active)
			{
				TriggerActiveEvent();
			}
			else if (m_mapGridDetail.Trigger == EDungeonEventTrigger.Passive)
			{
				TriggerPassiveEvent();
			}
		}
	}

	private void TriggerActiveEvent()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		progressPosition = new Vector3(m_eventPos.x, m_eventPos.y + 1.5f, progressPosition.z);
		actionPerformer.StartAction(actionTime, progressPosition, OnActionEnd);
	}

	private void OnActionEnd()
	{
		zbDungeonMgr.Instance.dungeonHandler.EventTrigger_Req(m_mapCellDetail.CellId, m_mapGridDetail.GridId);
	}

	private void TriggerPassiveEvent()
	{
		zbDungeonMgr.Instance.dungeonHandler.EventTrigger_Req(m_mapCellDetail.CellId, m_mapGridDetail.GridId);
	}

	public override void OnUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		zbDungeonMgr.Instance.dungeonWnd.SetActionProgress(actionPerformer.IsRunning, actionPerformer.Progress, actionPerformer.ProgressPosition);
	}
}
