using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionHero : MonoBehaviour
{
	private Transform m_root;

	private zbPlayerAvatar m_playerAvatar;

	public bool m_isLeader;

	public int curFloor;

	[CompilerGenerated]
	private Vector2 _003Cfloor1GatePos_003Ek__BackingField;

	private float minX;

	private float maxX;

	private zbLegionHeroHud m_heroHud;

	public LegionMemberDetail legionMemberDetail { get; private set; }

	public float[] floorPosY { get; private set; }

	public Vector2 floor1GatePos
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003Cfloor1GatePos_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003Cfloor1GatePos_003Ek__BackingField = value;
		}
	}

	public List<Vector2> floorElePos { get; private set; }

	public float moveSpeed { get; private set; }

	public zbLegionHeroMachine fsm { get; private set; }

	public void Init(LegionMemberDetail legionMemberDetail)
	{
		RefreshMemberInfo(legionMemberDetail);
		SetRandPos();
		fsm = new zbLegionHeroMachine();
		fsm.Init(this);
		fsm.SetState(ELegionHeroState.Idle);
	}

	public void RefreshMemberInfo(LegionMemberDetail legionMemberDetail)
	{
		this.legionMemberDetail = legionMemberDetail;
		bool flag = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId) == ELegionOfficial.Leader;
		bool flag2 = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser;
		m_isLeader = flag | flag2;
		InitHud();
		m_root = ((Component)this).transform.Find("root");
		m_playerAvatar = ((Component)m_root.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.RefreshCustom(legionMemberDetail.RoleBaseInfo.AvatarId, legionMemberDetail.RoleBaseInfo.GunId);
		}
	}

	public void InitHud()
	{
		VisualElement rootLegionHeroHud = ((Component)this).GetComponentInParent<zbLegionWnd>().rootLegionHeroHud;
		if (m_heroHud == null)
		{
			m_heroHud = new zbLegionHeroHud(rootLegionHeroHud, this);
		}
		m_heroHud.Init(legionMemberDetail);
	}

	public float GetRandPosX()
	{
		return Random.Range(minX, maxX);
	}

	public void SetFloor(int floor)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		curFloor = floor;
		Vector3 position = ((Component)this).transform.position;
		position.y = floorPosY[curFloor];
		((Component)this).transform.position = position;
	}

	private void SetRandPos()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float randPosX = GetRandPosX();
		if (m_isLeader)
		{
			SetFloor(2);
		}
		else
		{
			SetFloor((!(Random.value < 0.5f)) ? 1 : 0);
		}
		float num = Random.Range(1, 100);
		((Component)this).transform.position = new Vector3(randPosX, ((Component)this).transform.position.y, num);
	}

	public void PlayAnimation(string animName, bool loop = true)
	{
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.SetAnimation(0, animName, loop);
		}
	}

	public void SetFacing(bool left)
	{
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.SetFacing(left);
		}
	}

	public void OnClick()
	{
		if (legionMemberDetail != null)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)legionMemberDetail.RoleBaseInfo.RoleId);
		}
	}

	public void SetHudVisible(bool visible)
	{
		if (m_heroHud != null)
		{
			m_heroHud.SetHudVisible(visible);
		}
	}

	public void SetPlayerVisible(bool visible)
	{
		((Component)m_root).gameObject.SetActive(visible);
		SetHudVisible(visible);
	}

	private void OnDestroy()
	{
		if (m_heroHud != null)
		{
			m_heroHud.RemoveHud();
		}
	}

	private void UpdateHeroHudPos()
	{
		if (m_heroHud != null)
		{
			m_heroHud.UpdatePos();
		}
	}

	private void Update()
	{
		if (fsm != null)
		{
			fsm.Update();
		}
		UpdateHeroHudPos();
	}

	public zbLegionHero()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		floorPosY = new float[3] { -5.12f, -1.3f, 2.46f };
		floor1GatePos = new Vector2(-2.2f, -5.12f);
		floorElePos = new List<Vector2>
		{
			new Vector2(2.4f, -5.12f),
			new Vector2(2.4f, -1.3f),
			new Vector2(2.4f, 2.46f)
		};
		minX = -2.5f;
		maxX = 2.5f;
		moveSpeed = 0.5f;
		((MonoBehaviour)this)._002Ector();
	}
}
