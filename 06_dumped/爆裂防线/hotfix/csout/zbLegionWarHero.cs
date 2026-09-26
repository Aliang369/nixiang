using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarHero : MonoBehaviour
{
	private Transform m_root;

	private float m_spineAvatarScale;

	private GameObject m_zombieObject;

	private SkeletonAnimation m_skeletonAnimation;

	private GameObject m_fighting;

	private GameObject m_baohuzhao;

	private zbPlayerAvatar m_playerAvatar { get; set; }

	public LegionWarPointPlayerDetail legionWarPointPlayerDetail { get; private set; }

	public List<Vector2> leftZonePoint { get; private set; }

	public List<Vector2> middZonePoint { get; private set; }

	public List<Vector2> rightZonePoint { get; private set; }

	public float moveSpeed { get; private set; }

	public zbLegionWarHeroHud m_heroHud { get; private set; }

	protected void Awake()
	{
		m_zombieObject = ((Component)((Component)this).transform.Find("zombie")).gameObject;
		m_skeletonAnimation = ((Component)m_zombieObject.transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_baohuzhao = ((Component)((Component)this).transform.Find("baoHuZhao")).gameObject;
		m_fighting = ((Component)((Component)this).transform.Find("fighting")).gameObject;
	}

	public void Init(LegionWarPointPlayerDetail legionWarPointPlayerDetail, int index)
	{
		RefreshMemberInfo(legionWarPointPlayerDetail);
		SetRandPos();
	}

	public void RefreshMemberInfo(LegionWarPointPlayerDetail _legionWarPointPlayerDetail)
	{
		legionWarPointPlayerDetail = _legionWarPointPlayerDetail;
		InitHud();
		m_playerAvatar = ((Component)((Component)this).transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.RefreshCustom(legionWarPointPlayerDetail.RoleBaseInfo.AvatarId, legionWarPointPlayerDetail.RoleBaseInfo.GunId);
		}
		m_zombieObject.SetActive(false);
		((Component)m_playerAvatar).gameObject.SetActive(true);
		zbUtil.SetActive(m_fighting, active: false);
		foreach (LegionWarFightingDetail fighting in zbSelf.Instance.legionWarHandler.legionWarPointInfo.FightingList)
		{
			if (fighting.AttackerRoleId == legionWarPointPlayerDetail.RoleBaseInfo.RoleId || fighting.DefenderRoleId == legionWarPointPlayerDetail.RoleBaseInfo.RoleId)
			{
				zbUtil.SetActive(m_fighting, active: true);
			}
		}
	}

	public void InitHud()
	{
		VisualElement root = UQueryExtensions.Q<VisualElement>(((Component)this).GetComponentInParent<zbLegionWarFightWnd>().GetRootElement(), "hudRoot", (string)null);
		if (m_heroHud == null)
		{
			m_heroHud = new zbLegionWarHeroHud(root, (MonoBehaviour)(object)this);
		}
		m_heroHud.Init(legionWarPointPlayerDetail);
	}

	private void SetRandPos()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		long roleId = legionWarPointPlayerDetail.RoleBaseInfo.RoleId;
		ELegionWarForce forceId = legionWarPointPlayerDetail.RoleStateInfo.ForceId;
		ELegionWarForce legionForceId = zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId;
		if (!zbLegionWarFightMgr.Instance.heroPositions.TryGetValue(roleId, out var value))
		{
			List<Vector2> points = ((roleId < 100000) ? middZonePoint : ((forceId != legionForceId) ? rightZonePoint : leftZonePoint));
			value = zbSelf.Instance.legionWarHandler.GetRandomPoint(points);
			zbLegionWarFightMgr.Instance.heroPositions[roleId] = value;
		}
		float num = ((forceId == legionForceId) ? (0f - m_spineAvatarScale) : m_spineAvatarScale);
		((Component)this).transform.localScale = new Vector3(num, m_spineAvatarScale, 1f);
		((Component)this).transform.position = new Vector3(value.x, value.y, value.y / 10f * 2f);
	}

	private void OnDestroy()
	{
		if (m_heroHud != null)
		{
			m_heroHud.RemoveHud();
			m_heroHud = null;
		}
	}

	private void Update()
	{
		if (m_heroHud != null)
		{
			m_heroHud.Update();
		}
		zbUtil.SetActive(m_baohuzhao, active: false);
		if (legionWarPointPlayerDetail.RoleStateInfo.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUtil.SetActive(m_baohuzhao, active: true);
		}
	}

	public zbLegionWarHero()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		m_spineAvatarScale = 0.3f;
		leftZonePoint = new List<Vector2>
		{
			new Vector2(-4.8f, 4.7999997f),
			new Vector2(-3.4f, 4.7999997f),
			new Vector2(-4f, -3.6000004f),
			new Vector2(-5.4f, -3.6000004f)
		};
		middZonePoint = new List<Vector2>
		{
			new Vector2(-1.8000002f, 4.7999997f),
			new Vector2(1.8000002f, 4.7999997f),
			new Vector2(1.8000002f, -3.6000004f),
			new Vector2(-1.8000002f, -3.6000004f)
		};
		rightZonePoint = new List<Vector2>
		{
			new Vector2(4.8f, 4.7999997f),
			new Vector2(3.4f, 4.7999997f),
			new Vector2(4f, -3.6000004f),
			new Vector2(5.4f, -3.6000004f)
		};
		moveSpeed = 0.5f;
		((MonoBehaviour)this)._002Ector();
	}
}
