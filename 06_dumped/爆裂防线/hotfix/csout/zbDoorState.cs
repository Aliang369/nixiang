using UnityEngine;

public class zbDoorState
{
	protected EDoorState m_eState;

	protected zbDoorStateMachine m_fsm;

	protected static float m_doorDetectRange = 0.7f;

	public zbDoorState(zbDoorStateMachine fsm, EDoorState eState)
	{
		m_fsm = fsm;
		m_eState = eState;
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnEnable()
	{
	}

	public bool IsWorkerNearDoor()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (m_fsm.isLeftDoor ? m_fsm.room.GetLeftDoorPos() : m_fsm.room.GetRightDoorPos());
		bool flag = false;
		if (!flag)
		{
			foreach (zbWorker worker in zbHomeMgr.Instance.workerMgr.workerList)
			{
				Vector3 position = ((Component)worker).transform.position;
				float num = Mathf.Abs(position.x - val.x);
				float num2 = Mathf.Abs(position.y - val.y);
				if (num <= m_doorDetectRange && num2 <= m_doorDetectRange)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			foreach (zbLeaver worker2 in zbHomeMgr.Instance.leaverMgr.workerList)
			{
				Vector3 position2 = ((Component)worker2).transform.position;
				float num3 = Mathf.Abs(position2.x - val.x);
				float num4 = Mathf.Abs(position2.y - val.y);
				if (num3 <= m_doorDetectRange && num4 <= m_doorDetectRange)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag && m_fsm.room.roomData.Y > 0)
		{
			foreach (zbRoomHero roomHero in zbHomeMgr.Instance.roomHeroMgr.roomHeroList)
			{
				Vector3 position3 = ((Component)roomHero).transform.position;
				float num5 = Mathf.Abs(position3.x - val.x);
				float num6 = Mathf.Abs(position3.y - val.y);
				if (num5 <= m_doorDetectRange && num6 <= m_doorDetectRange)
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}
}
