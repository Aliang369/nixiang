using System.Collections.Generic;
using UnityEngine;

public class zbRoomDefine
{
	private static zbRoomDefine sInstance;

	private Dictionary<int, float> m_dicRoomPosX_Level1;

	private Dictionary<int, float> m_dicRoomPosX_Level2;

	private Dictionary<int, float> m_dicRoomPosX_Level3;

	public readonly Vector3 RecruiterOuterPosition;

	public readonly Vector3 RecruiterEntryPosition;

	public readonly Vector3 GatherWildLeftPosition;

	public readonly Vector3 GatherWildPosition;

	public Vector3 ZombieSpawnPosition;

	public readonly Vector3 ZombieTargetPosition;

	public readonly Vector3 RoomHeroElevatorTopPosition;

	private readonly float innerRunSpeed;

	private readonly float innerWalkSpeed;

	private readonly float innermoveSpeed;

	private readonly float innerRunZombieSpeed;

	private readonly float innerWalkZombieSpeed;

	public readonly float runSpeed;

	public readonly float walkSpeed;

	public readonly float moveSpeed;

	public readonly float runZombieSpeed;

	public readonly float walkZombieSpeed;

	public static zbRoomDefine Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbRoomDefine();
				sInstance.Init();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		float num = 1.5f;
		float num2 = 3.1f;
		float num3 = 4.7f;
		m_dicRoomPosX_Level1[1] = -9.1f;
		m_dicRoomPosX_Level1[2] = -5.9f;
		m_dicRoomPosX_Level1[3] = -2.7f;
		m_dicRoomPosX_Level1[4] = 2.7f;
		m_dicRoomPosX_Level1[5] = 5.9f;
		m_dicRoomPosX_Level1[6] = 9.1f;
		m_dicRoomPosX_Level2[1] = m_dicRoomPosX_Level1[1] - num + num2;
		m_dicRoomPosX_Level2[2] = m_dicRoomPosX_Level1[2] - num + num2;
		m_dicRoomPosX_Level2[4] = m_dicRoomPosX_Level1[4] - num + num2;
		m_dicRoomPosX_Level2[5] = m_dicRoomPosX_Level1[5] - num + num2;
		m_dicRoomPosX_Level3[1] = m_dicRoomPosX_Level1[1] - num + num3;
		m_dicRoomPosX_Level3[4] = m_dicRoomPosX_Level1[4] - num + num3;
	}

	public float GetRoomPostionX(int length, int x)
	{
		return length switch
		{
			3 => m_dicRoomPosX_Level3[x], 
			2 => m_dicRoomPosX_Level2[x], 
			_ => m_dicRoomPosX_Level1[x], 
		};
	}

	public bool IsValidX(int length, int x)
	{
		return length switch
		{
			3 => m_dicRoomPosX_Level3.ContainsKey(x), 
			2 => m_dicRoomPosX_Level2.ContainsKey(x), 
			1 => m_dicRoomPosX_Level1.ContainsKey(x), 
			_ => false, 
		};
	}

	public float GetAnimationTimeScale(string animationName)
	{
		float result = 1f;
		switch (animationName)
		{
		case "walk":
			result = walkSpeed / innerWalkSpeed;
			break;
		case "run":
			result = runSpeed / innerRunSpeed;
			break;
		case "move":
			result = moveSpeed / innermoveSpeed;
			break;
		case "walk_dejection":
			result = walkSpeed / innerWalkSpeed;
			break;
		case "walk_zombie":
			result = walkZombieSpeed / innerWalkZombieSpeed;
			break;
		case "run_zombie":
			result = runZombieSpeed / innerRunZombieSpeed;
			break;
		}
		return result;
	}

	public bool IsInLeftWild(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (position.x < RecruiterEntryPosition.x + 0.5f && position.y >= RecruiterEntryPosition.y - 0.5f)
		{
			return position.y <= RecruiterEntryPosition.y + 0.5f;
		}
		return false;
	}

	public bool IsInRightWild(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (position.x > GatherWildLeftPosition.x && position.y >= GatherWildLeftPosition.y - 0.5f)
		{
			return position.y <= GatherWildLeftPosition.y + 0.5f;
		}
		return false;
	}

	public bool ShouldExitFromLeft(zbRoom nearRoom)
	{
		if (nearRoom.roomData.X <= 3)
		{
			return false;
		}
		return true;
	}

	public bool ShouldEnterFromLeft(zbRoom targetNearRoom)
	{
		if (targetNearRoom.roomData.X <= 3)
		{
			return false;
		}
		return true;
	}

	public zbRoomDefine()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		m_dicRoomPosX_Level1 = new Dictionary<int, float>();
		m_dicRoomPosX_Level2 = new Dictionary<int, float>();
		m_dicRoomPosX_Level3 = new Dictionary<int, float>();
		RecruiterOuterPosition = new Vector3(-18.5f, -13.4f, 0f);
		RecruiterEntryPosition = new Vector3(-4.5f, -13.45f, 0f);
		GatherWildLeftPosition = new Vector3(19f, -13.4f, 0f);
		GatherWildPosition = new Vector3(27f, -13.4f, 0f);
		ZombieSpawnPosition = new Vector3(-18.5f, -13.41f, 0f);
		ZombieTargetPosition = new Vector3(-11.2f, -13.41f, 0f);
		RoomHeroElevatorTopPosition = new Vector3(7.6f, -4f, 0f);
		innerRunSpeed = 0.95f;
		innerWalkSpeed = 0.25f;
		innermoveSpeed = 0.1f;
		innerRunZombieSpeed = 0.33f;
		innerWalkZombieSpeed = 0.083f;
		runSpeed = 1.5f;
		walkSpeed = 0.33f;
		moveSpeed = 0.2f;
		runZombieSpeed = 0.95f;
		walkZombieSpeed = 0.11f;
		base._002Ector();
	}
}
