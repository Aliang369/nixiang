using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonRoom : MonoBehaviour
{
	[NonSerialized]
	public zbDungeonFloor floor;

	public DungeonMapCellDetail dungeonMapCellDetail;

	public zbDungeonCellTypeCsvRecord dungeonCellTypeCsvRecord;

	public zbDungeonCellStyleCsvRecord dungeonCellStyleCsvRecord;

	private SpriteRenderer m_spriteRenderer;

	private SpriteRenderer m_spriteVisible;

	private SpriteRenderer m_spriteGap;

	protected SkeletonAnimation m_spineAmbient;

	protected SkeletonAnimation m_spineLeftDoor;

	protected SkeletonAnimation m_spineRightDoor;

	protected SkeletonAnimation m_spineElevator;

	protected SkeletonAnimation m_spineSeal;

	protected Transform m_spineEventRoot;

	public SkeletonAnimation[] spineEvent;

	private SkeletonAnimation[] m_spineEye;

	private string m_spineAmbientPath = "";

	private string m_spineLeftDoorPath = "";

	private string m_spineRightDoorPath = "";

	private string m_spineElevatorPath = "";

	private bool m_showEye;

	private bool isRoomStateChange;

	private EDungeonMapCellState m_lastCellState;

	private zbDungeonRoomEvent m_dungeonRoomEvent;

	protected virtual void Awake()
	{
		Transform val = ((Component)this).transform.Find("root");
		m_spriteRenderer = ((Component)val.Find("roombg")).GetComponent<SpriteRenderer>();
		m_spriteVisible = ((Component)val.Find("roomvisible")).GetComponent<SpriteRenderer>();
		m_spineAmbient = ((Component)val.Find("spineAmbient")).GetComponent<SkeletonAnimation>();
		m_spineLeftDoor = ((Component)val.Find("spineLeftDoor")).GetComponent<SkeletonAnimation>();
		m_spineRightDoor = ((Component)val.Find("spineRightDoor")).GetComponent<SkeletonAnimation>();
		m_spineElevator = ((Component)val.Find("spineElevator")).GetComponent<SkeletonAnimation>();
		m_spineSeal = ((Component)val.Find("spineSeal")).GetComponent<SkeletonAnimation>();
		m_spineEventRoot = val.Find("spineEvent");
		spineEvent = (SkeletonAnimation[])(object)new SkeletonAnimation[3];
		for (int i = 0; i < 3; i++)
		{
			string text = $"event{i + 1}";
			spineEvent[i] = ((Component)m_spineEventRoot.Find(text)).GetComponent<SkeletonAnimation>();
		}
		m_spriteGap = ((Component)val.Find("gap")).GetComponent<SpriteRenderer>();
		m_dungeonRoomEvent = new zbDungeonRoomEvent(this);
	}

	public void ResetRoomInfo()
	{
		dungeonMapCellDetail = null;
	}

	public void Create(zbDungeonFloor _floor, DungeonMapCellDetail cellDetail)
	{
		if (cellDetail != null)
		{
			floor = _floor;
			isRoomStateChange = false;
			if (dungeonMapCellDetail != null)
			{
				isRoomStateChange = dungeonMapCellDetail.State != cellDetail.State;
				m_lastCellState = dungeonMapCellDetail.State;
			}
			dungeonMapCellDetail = cellDetail;
			dungeonCellTypeCsvRecord = zbDataConfig.Instance.dungeonCellTypeCsvTable.GetRecordByKey(dungeonMapCellDetail.CellType);
			dungeonCellStyleCsvRecord = zbDataConfig.Instance.dungeonCellStyleCsvTable.GetRecordByKey(dungeonMapCellDetail.CellStyle);
			m_showEye = false;
			RefreshRoomDetail();
			SpawnEye();
		}
	}

	private void SpawnEye()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (!m_showEye)
		{
			if (m_spineEye != null && m_spineEye.Length != 0)
			{
				SkeletonAnimation[] spineEye = m_spineEye;
				for (int i = 0; i < spineEye.Length; i++)
				{
					((Component)spineEye[i]).gameObject.SetActive(false);
				}
			}
			return;
		}
		int length = dungeonCellTypeCsvRecord.length;
		if (m_spineEye == null)
		{
			m_spineEye = (SkeletonAnimation[])(object)new SkeletonAnimation[0];
		}
		int num = m_spineEye.Length;
		if (num >= length)
		{
			for (int j = 0; j < num; j++)
			{
				((Component)m_spineEye[j]).gameObject.SetActive(j < length);
			}
		}
		else
		{
			int num2 = length - num;
			GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/dungeon/dungeoneye.prefab");
			List<SkeletonAnimation> list = new List<SkeletonAnimation>(m_spineEye);
			for (int k = 0; k < num2; k++)
			{
				GameObject val2 = Object.Instantiate<GameObject>(val, Vector3.zero, Quaternion.identity, ((Component)m_spriteVisible).transform.parent);
				SkeletonAnimation component = val2.GetComponent<SkeletonAnimation>();
				if ((Object)(object)component != (Object)null)
				{
					list.Add(component);
				}
				else
				{
					Object.Destroy((Object)(object)val2);
				}
			}
			m_spineEye = list.ToArray();
			for (int l = 0; l < m_spineEye.Length; l++)
			{
				((Component)m_spineEye[l]).gameObject.SetActive(true);
			}
		}
		SetEyePositions(length);
	}

	private void SetEyePositions(int count)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (count <= 0 || m_spineEye == null || m_spineEye.Length == 0)
		{
			return;
		}
		float num = 1.5f;
		Vector3 localPosition = default(Vector3);
		for (int i = 0; i < count && i < m_spineEye.Length; i++)
		{
			float num2 = 0f;
			if (count % 2 == 1)
			{
				if (i == 0)
				{
					num2 = 0f;
				}
				else
				{
					int num3 = i;
					int num4 = (num3 + 1) / 2;
					num2 = (float)(((num3 % 2 != 1) ? 1 : (-1)) * num4) * num;
				}
			}
			else
			{
				int num5 = i;
				int num6 = num5 / 2 + 1;
				num2 = (float)(((num5 % 2 != 0) ? 1 : (-1)) * num6) * num;
			}
			((Vector3)(ref localPosition))._002Ector(num2, 0f, ((Component)m_spineEye[i]).transform.localPosition.z);
			((Component)m_spineEye[i]).transform.localPosition = localPosition;
		}
	}

	private void RefreshRoomDetail()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		if (dungeonMapCellDetail != null)
		{
			float num = (float)(dungeonMapCellDetail.X - 1) * 3f + (float)(dungeonMapCellDetail.X - 1) * 0.2f;
			((Component)this).transform.localPosition = new Vector3(num, 0f, 0f);
			((Component)this).transform.localScale = Vector3.one;
			Transform val = ((Component)this).transform.Find("root");
			val.localPosition = new Vector3(GetRoomWidth() / 2f, val.localPosition.y, val.localPosition.z);
			Vector3 localPosition = ((Component)m_spriteGap).transform.localPosition;
			((Component)m_spriteGap).gameObject.SetActive(false);
			if (dungeonMapCellDetail.State != EDungeonMapCellState.Invisible && zbDungeonMgr.Instance.dungeonHandler.GetNextCellDetail(dungeonMapCellDetail, left: false) != null)
			{
				((Component)m_spriteGap).gameObject.SetActive(true);
				((Component)m_spriteGap).transform.localPosition = new Vector3((GetRoomWidth() + 0.2f) / 2f, localPosition.y, localPosition.z);
			}
			((Component)m_spriteVisible).gameObject.SetActive(false);
			if (!IsElevator() && dungeonMapCellDetail.State == EDungeonMapCellState.Visible)
			{
				RefreshVisibleRoom();
			}
			else
			{
				RefreshNormalRoom();
			}
		}
	}

	private void RefreshVisibleRoom()
	{
		SetSpineVisible(bVisible: false);
		string strAssetPath = "ui/image/dungeon/roombg/empty.jpg";
		if (dungeonCellTypeCsvRecord.length == 2)
		{
			strAssetPath = "ui/image/dungeon/roombg/empty2.jpg";
		}
		else if (dungeonCellTypeCsvRecord.length >= 3)
		{
			strAssetPath = "ui/image/dungeon/roombg/empty3.jpg";
		}
		Sprite sprite = zbAssetLoader.Instance.LoadSpriteAsset(strAssetPath);
		m_spriteRenderer.sprite = sprite;
		string strAssetPath2 = "ui/image/dungeon/roombg/room_no.png";
		if (zbDungeonMgr.Instance.dungeonHandler.IsRoomHaveBoss(dungeonMapCellDetail))
		{
			strAssetPath2 = "ui/image/dungeon/roombg/room_boss.png";
		}
		if (!IsElevator())
		{
			Sprite sprite2 = zbAssetLoader.Instance.LoadSpriteAsset(strAssetPath2);
			m_spriteVisible.sprite = sprite2;
			((Component)m_spriteVisible).gameObject.SetActive(true);
			m_showEye = true;
		}
	}

	private void RefreshNormalRoom()
	{
		SetSpineVisible(bVisible: true);
		string bg = dungeonCellStyleCsvRecord.bg;
		Sprite sprite = zbAssetLoader.Instance.LoadSpriteAsset(bg);
		m_spriteRenderer.sprite = sprite;
		string ambient = dungeonCellStyleCsvRecord.ambient;
		ambient += "_SkeletonData.asset";
		if (!m_spineAmbientPath.Equals(ambient))
		{
			m_spineAmbientPath = ambient;
			zbUtil.RefreshSpine(m_spineAmbient, ambient, "ambient", loop: true);
		}
		((Component)m_spineLeftDoor).gameObject.SetActive(IsHaveDoor(isLeft: true));
		string leftdoor = dungeonCellStyleCsvRecord.leftdoor;
		leftdoor += "_SkeletonData.asset";
		if (!m_spineLeftDoorPath.Equals(leftdoor))
		{
			m_spineLeftDoorPath = leftdoor;
			zbUtil.RefreshSpine(m_spineLeftDoor, leftdoor);
		}
		((Component)m_spineRightDoor).gameObject.SetActive(IsHaveDoor(isLeft: false));
		string rightdoor = dungeonCellStyleCsvRecord.rightdoor;
		rightdoor += "_SkeletonData.asset";
		if (!m_spineRightDoorPath.Equals(rightdoor))
		{
			m_spineRightDoorPath = rightdoor;
			zbUtil.RefreshSpine(m_spineRightDoor, rightdoor);
		}
		InitSpineAnimation();
		if (IsElevator())
		{
			string middledoor = dungeonCellStyleCsvRecord.middledoor;
			if (!string.IsNullOrEmpty(middledoor))
			{
				middledoor += "_SkeletonData.asset";
				if (!m_spineElevatorPath.Equals(middledoor))
				{
					m_spineElevatorPath = middledoor;
					zbUtil.RefreshSpine(m_spineElevator, middledoor, "middle_idle_close", loop: true);
				}
			}
		}
		((Component)m_spineElevator).gameObject.SetActive(IsElevator());
		((Component)m_spineSeal).gameObject.SetActive(IsElevator());
		m_dungeonRoomEvent.RefreshEvent();
	}

	private void InitSpineAnimation()
	{
		if (IsElevator())
		{
			InitElevatorAnimation();
			return;
		}
		if (isRoomStateChange)
		{
			PlayDoorAnimationStateChange();
			return;
		}
		SetDooorAnimation(isLeft: true, dungeonMapCellDetail.State == EDungeonMapCellState.LeftOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
		SetDooorAnimation(isLeft: false, dungeonMapCellDetail.State == EDungeonMapCellState.RightOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
	}

	private void InitElevatorAnimation()
	{
		if (isRoomStateChange)
		{
			PlayDoorAnimationStateChange();
			return;
		}
		SetDooorAnimation(isLeft: true, dungeonMapCellDetail.State == EDungeonMapCellState.LeftOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
		SetDooorAnimation(isLeft: false, dungeonMapCellDetail.State == EDungeonMapCellState.RightOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
		PlayeElevatorSealAnimation();
	}

	private void PlayDoorAnimationStateChange()
	{
		if (dungeonMapCellDetail.State == EDungeonMapCellState.LeftOpen)
		{
			PlayDoorAnimation(isLeft: true, isOpen: true);
			SetDooorAnimation(isLeft: false, isOpen: false);
		}
		else if (dungeonMapCellDetail.State == EDungeonMapCellState.RightOpen)
		{
			PlayDoorAnimation(isLeft: false, isOpen: true);
			SetDooorAnimation(isLeft: true, isOpen: false);
		}
		else if (dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen)
		{
			if (m_lastCellState == EDungeonMapCellState.RightOpen)
			{
				PlayDoorAnimation(isLeft: true, isOpen: true);
				SetDooorAnimation(isLeft: false, isOpen: true);
			}
			else if (m_lastCellState == EDungeonMapCellState.LeftOpen)
			{
				PlayDoorAnimation(isLeft: false, isOpen: true);
				SetDooorAnimation(isLeft: true, isOpen: true);
			}
		}
		else if (dungeonMapCellDetail.State == EDungeonMapCellState.Activate)
		{
			SetDooorAnimation(isLeft: true, dungeonMapCellDetail.State == EDungeonMapCellState.LeftOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
			SetDooorAnimation(isLeft: false, dungeonMapCellDetail.State == EDungeonMapCellState.RightOpen || dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen);
		}
		if (IsElevator() && m_lastCellState == EDungeonMapCellState.Visible)
		{
			PlayElevatorActive();
		}
	}

	private void SetSpineVisible(bool bVisible)
	{
		((Component)m_spineAmbient).gameObject.SetActive(bVisible);
		((Component)m_spineLeftDoor).gameObject.SetActive(bVisible);
		((Component)m_spineRightDoor).gameObject.SetActive(bVisible);
		((Component)m_spineElevator).gameObject.SetActive(bVisible);
		((Component)m_spineEventRoot).gameObject.SetActive(bVisible);
		((Component)m_spineSeal).gameObject.SetActive(IsElevator() & bVisible);
	}

	public virtual float GetRoomWidth()
	{
		if (dungeonCellTypeCsvRecord == null)
		{
			return 0f;
		}
		return (float)dungeonCellTypeCsvRecord.length * 3f + (float)(dungeonCellTypeCsvRecord.length - 1) * 0.2f;
	}

	public Vector3 GetCenterPos()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float roomWidth = GetRoomWidth();
		return ((Component)this).transform.position + new Vector3(roomWidth / 2f, 0f, 0f);
	}

	public bool IsInside(float x, float y)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (y - ((Component)this).transform.position.y < 2.2f)
		{
			float num = ((Component)this).transform.position.x + 0.3f;
			float num2 = ((Component)this).transform.position.x + GetRoomWidth() - 0.3f;
			if (x >= num)
			{
				return x <= num2;
			}
			return false;
		}
		return false;
	}

	public bool IsElevator()
	{
		return dungeonMapCellDetail.FunType == EDungeonMapCellFunType.Elevator;
	}

	public bool IsSpawn()
	{
		return dungeonMapCellDetail.FunType == EDungeonMapCellFunType.Spawn;
	}

	public bool CanMove(float targetX, float targetY)
	{
		if (!IsInside(targetX, targetY))
		{
			return false;
		}
		return true;
	}

	public Vector3 GetLeftDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public Vector3 GetRightDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(((Component)this).transform.position.x + GetRoomWidth(), ((Component)this).transform.position.y, ((Component)this).transform.position.z);
	}

	public Vector3 GetDoorPos(bool isLeft)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (!isLeft)
		{
			return GetRightDoorPos();
		}
		return GetLeftDoorPos();
	}

	public bool IsHaveDoor(bool isLeft)
	{
		if (dungeonCellTypeCsvRecord == null)
		{
			return false;
		}
		if (isLeft)
		{
			return dungeonCellTypeCsvRecord.leftDoor == 1;
		}
		return dungeonCellTypeCsvRecord.rightDoor == 1;
	}

	public DungeonMapGridDetail GetRoomGridIDetailFromPositionX(float x)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((x - ((Component)this).transform.position.x) / 3f);
		if (dungeonMapCellDetail != null && num >= 0 && num < dungeonMapCellDetail.GridList.Count)
		{
			return dungeonMapCellDetail.GridList[num];
		}
		return null;
	}

	public int GetGridOfRoom(int gridId)
	{
		int num = zbDungeonMgr.Instance.dungeonHandler.dungeonMapInfo?.MaxX ?? 25;
		int num2 = gridId % num;
		num2 = ((num2 == 0) ? num : num2);
		return num2 - dungeonMapCellDetail.X;
	}

	public SkeletonAnimation GetSkeletonAnimationInGrid(int gridId)
	{
		int gridOfRoom = GetGridOfRoom(gridId);
		if (gridOfRoom >= 0 && gridOfRoom < spineEvent.Length)
		{
			return spineEvent[gridOfRoom];
		}
		return null;
	}

	public void PlayElevatorAnimation(bool isOpen)
	{
		string text = (isOpen ? "middle_anim_open" : "middle_anim_close");
		if (zbUtil.CheckAnimationNameExist(m_spineElevator, text))
		{
			m_spineElevator.AnimationState.SetAnimation(0, text, false);
		}
	}

	public void PlayElevatorActive()
	{
		if (zbUtil.CheckAnimationNameExist(m_spineSeal, "anim_remove"))
		{
			m_spineSeal.AnimationState.SetAnimation(0, "anim_remove", false);
		}
		if (zbUtil.CheckAnimationNameExist(m_spineSeal, "idle_hide"))
		{
			m_spineSeal.AnimationState.AddAnimation(0, "idle_hide", true, 0.5f);
		}
	}

	public void PlayeElevatorSealAnimation()
	{
		if (dungeonMapCellDetail.State == EDungeonMapCellState.Visible)
		{
			if (zbUtil.CheckAnimationNameExist(m_spineSeal, "idle_show"))
			{
				m_spineSeal.AnimationState.SetAnimation(0, "idle_show", true);
			}
		}
		else if (zbUtil.CheckAnimationNameExist(m_spineSeal, "idle_hide"))
		{
			m_spineSeal.AnimationState.SetAnimation(0, "idle_hide", true);
		}
	}

	private string GetDoorAnimationName(bool isLeft, bool isOpen)
	{
		string obj = (isLeft ? "left_idle" : "right_idle");
		string text = (isOpen ? "open" : "close");
		return obj + "_" + text;
	}

	public void SetDooorAnimation(bool isLeft, bool isOpen)
	{
		string doorAnimationName = GetDoorAnimationName(isLeft, isOpen);
		SkeletonAnimation val = (isLeft ? m_spineLeftDoor : m_spineRightDoor);
		if (!isLeft)
		{
			_ = m_spineRightDoorPath;
		}
		else
		{
			_ = m_spineLeftDoorPath;
		}
		if (zbUtil.CheckAnimationNameExist(val, doorAnimationName))
		{
			val.AnimationState.SetAnimation(0, doorAnimationName, true);
		}
	}

	public void PlayDoorAnimation(bool isLeft, bool isOpen)
	{
		string obj = (isLeft ? "left_anim" : "right_anim");
		string text = (isOpen ? "open" : "close");
		string text2 = obj + "_" + text;
		SkeletonAnimation val = (isLeft ? m_spineLeftDoor : m_spineRightDoor);
		if (!isLeft)
		{
			_ = m_spineRightDoorPath;
		}
		else
		{
			_ = m_spineLeftDoorPath;
		}
		if (zbUtil.CheckAnimationNameExist(val, text2))
		{
			val.AnimationState.SetAnimation(0, text2, false);
		}
		string doorAnimationName = GetDoorAnimationName(isLeft, isOpen);
		if (zbUtil.CheckAnimationNameExist(val, doorAnimationName))
		{
			val.AnimationState.AddAnimation(0, doorAnimationName, true, 0.5f);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_spriteRenderer != (Object)null)
		{
			Sprite sprite = m_spriteRenderer.sprite;
			if ((Object)(object)sprite != (Object)null)
			{
				Object.Destroy((Object)(object)sprite);
			}
			m_spriteRenderer.sprite = null;
		}
	}
}
