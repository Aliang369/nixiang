using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarBattleHero : MonoBehaviour
{
	private GameObject m_fighting;

	private int index;

	private zbPlayerAvatar m_playerLeftAvatar { get; set; }

	private zbPlayerAvatar m_playerRightAvatar { get; set; }

	public LegionWarPointPlayerDetail playerLeftDetail { get; private set; }

	public LegionWarPointPlayerDetail playerRightDetail { get; private set; }

	public List<Vector2> middZonePoint { get; private set; }

	public float moveSpeed { get; private set; }

	public zbLegionWarHeroHud m_heroLeftHud { get; private set; }

	public zbLegionWarHeroHud m_heroRightHud { get; private set; }

	protected void Awake()
	{
		m_fighting = ((Component)((Component)this).transform.Find("fighting")).gameObject;
	}

	public void Init(LegionWarPointPlayerDetail playerAttackDetail, LegionWarPointPlayerDetail playerDefendDetail, int _index)
	{
		index = _index;
		RefreshMemberInfo(playerAttackDetail, playerDefendDetail);
		SetRandPos();
	}

	public void RefreshMemberInfo(LegionWarPointPlayerDetail _playerAttackDetail, LegionWarPointPlayerDetail _playerDefendDetail)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (_playerAttackDetail.RoleStateInfo.ForceId == zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
		{
			playerLeftDetail = _playerAttackDetail;
			playerRightDetail = _playerDefendDetail;
			m_fighting.transform.localScale = new Vector3(3f, 3f, 1f);
		}
		else
		{
			playerLeftDetail = _playerDefendDetail;
			playerRightDetail = _playerAttackDetail;
			m_fighting.transform.localScale = new Vector3(-3f, 3f, 1f);
		}
		_ = zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId;
		m_playerLeftAvatar = ((Component)((Component)this).transform.Find("leftPlayerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerLeftAvatar))
		{
			m_playerLeftAvatar.RefreshCustom(playerLeftDetail.RoleBaseInfo.AvatarId, playerLeftDetail.RoleBaseInfo.GunId);
		}
		m_playerRightAvatar = ((Component)((Component)this).transform.Find("rightPlayerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerRightAvatar))
		{
			m_playerRightAvatar.RefreshCustom(playerRightDetail.RoleBaseInfo.AvatarId, playerRightDetail.RoleBaseInfo.GunId);
		}
		InitHud();
	}

	public void InitHud()
	{
		VisualElement root = UQueryExtensions.Q<VisualElement>(((Component)this).GetComponentInParent<zbLegionWarFightWnd>().GetRootElement(), "hudRoot", (string)null);
		if (m_heroLeftHud == null)
		{
			m_heroLeftHud = new zbLegionWarHeroHud(root, (MonoBehaviour)(object)this);
		}
		m_heroLeftHud.Init(playerLeftDetail, 1f);
		if (m_heroRightHud == null)
		{
			m_heroRightHud = new zbLegionWarHeroHud(root, (MonoBehaviour)(object)this);
		}
		m_heroRightHud.Init(playerRightDetail, -1f);
	}

	private void SetRandPos()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = middZonePoint[index];
		((Component)this).transform.position = new Vector3(val.x, val.y, val.y / 10f * 2f);
	}

	private void OnDestroy()
	{
		if (m_heroLeftHud != null)
		{
			m_heroLeftHud.RemoveHud();
			m_heroLeftHud = null;
		}
		if (m_heroRightHud != null)
		{
			m_heroRightHud.RemoveHud();
			m_heroRightHud = null;
		}
	}

	private void Update()
	{
		if (m_heroLeftHud != null)
		{
			m_heroLeftHud.Update();
		}
		if (m_heroRightHud != null)
		{
			m_heroRightHud.Update();
		}
	}

	public zbLegionWarBattleHero()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		middZonePoint = new List<Vector2>
		{
			new Vector2(0f, 4.8f),
			new Vector2(0f, 3.6f),
			new Vector2(0f, 2.4f),
			new Vector2(0f, 1.2f),
			new Vector2(0f, 0f),
			new Vector2(0f, -1.2f),
			new Vector2(0f, -2.4f),
			new Vector2(0f, -3.6f),
			new Vector2(0f, -4.8f)
		};
		moveSpeed = 0.5f;
		((MonoBehaviour)this)._002Ector();
	}
}
