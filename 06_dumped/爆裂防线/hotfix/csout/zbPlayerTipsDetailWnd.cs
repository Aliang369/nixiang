using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbPlayerTipsDetailWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private VisualElement m_vwSuit;

	private ScrollView m_scViewSuit;

	private List<zbPlayerTipsSuitItem> m_SuitItem = new List<zbPlayerTipsSuitItem>();

	private VisualElement m_vwWall;

	private ScrollView m_scViewWall;

	private List<zbPlayerTipsWallItem> m_WallItem = new List<zbPlayerTipsWallItem>();

	private VisualElement m_vwGun;

	private ScrollView m_scViewGun;

	private List<zbPlayerTipsGunItem> m_GunItem = new List<zbPlayerTipsGunItem>();

	private VisualElement m_vwMecha;

	private ScrollView m_scViewMecha;

	private List<zbPlayerTipsMechaItem> m_MechaItem = new List<zbPlayerTipsMechaItem>();

	private VisualElement m_vwFoothold;

	private ScrollView m_scViewFoothold;

	private List<zbPlayerTipsFootholdItem> m_FootholdItem = new List<zbPlayerTipsFootholdItem>();

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_vwSuit = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_suit", (string)null);
		m_scViewSuit = UQueryExtensions.Q<ScrollView>(m_vwSuit, "list", (string)null);
		m_vwWall = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_wall", (string)null);
		m_scViewWall = UQueryExtensions.Q<ScrollView>(m_vwWall, "list", (string)null);
		m_vwGun = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_gun", (string)null);
		m_scViewGun = UQueryExtensions.Q<ScrollView>(m_vwGun, "list", (string)null);
		m_vwMecha = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_mecha", (string)null);
		m_scViewMecha = UQueryExtensions.Q<ScrollView>(m_vwMecha, "list", (string)null);
		m_vwFoothold = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_foothold", (string)null);
		m_scViewFoothold = UQueryExtensions.Q<ScrollView>(m_vwFoothold, "list", (string)null);
		AddDark();
	}

	public void RefreshCustom(RoleTipsDetail roleTipsDetail)
	{
		RefreshSuit(roleTipsDetail);
		RefreshWall(roleTipsDetail);
		RefreshGun(roleTipsDetail);
		RefreshMecha(roleTipsDetail);
		RefreshFoothold(roleTipsDetail);
	}

	public void RefreshSuit(RoleTipsDetail roleTipsDetail)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		m_SuitItem.Clear();
		((VisualElement)m_scViewSuit).Clear();
		List<PlayerSuitDetail> list = ((IEnumerable<PlayerSuitDetail>)roleTipsDetail.SuitList).ToList();
		list.Sort(delegate(PlayerSuitDetail p1, PlayerSuitDetail p2)
		{
			zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(p1.SuitId);
			int num = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(p2.SuitId).quality.CompareTo(recordByKey.quality);
			if (num != 0)
			{
				return num;
			}
			num = p2.Star.CompareTo(p1.Star);
			return (num != 0) ? num : p1.SuitId.CompareTo(p2.SuitId);
		});
		foreach (PlayerSuitDetail item in list)
		{
			if (item.SuitId != 1)
			{
				zbPlayerTipsSuitItem zbPlayerTipsSuitItem2 = new zbPlayerTipsSuitItem(this);
				m_SuitItem.Add(zbPlayerTipsSuitItem2);
				((VisualElement)m_scViewSuit).Add((VisualElement)(object)zbPlayerTipsSuitItem2);
				zbPlayerTipsSuitItem2.Refresh(item);
			}
		}
		m_vwSuit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_SuitItem.Count <= 0));
	}

	public void RefreshWall(RoleTipsDetail roleTipsDetail)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		m_WallItem.Clear();
		((VisualElement)m_scViewWall).Clear();
		List<WallDetail> list = ((IEnumerable<WallDetail>)roleTipsDetail.WallList).ToList();
		list.Sort(delegate(WallDetail p1, WallDetail p2)
		{
			zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(p1.WallId);
			int num = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(p2.WallId).quality.CompareTo(recordByKey.quality);
			if (num != 0)
			{
				return num;
			}
			num = p2.Star.CompareTo(p1.Star);
			return (num != 0) ? num : p1.WallId.CompareTo(p2.WallId);
		});
		foreach (WallDetail item in list)
		{
			zbPlayerTipsWallItem zbPlayerTipsWallItem2 = new zbPlayerTipsWallItem(this);
			m_WallItem.Add(zbPlayerTipsWallItem2);
			((VisualElement)m_scViewWall).Add((VisualElement)(object)zbPlayerTipsWallItem2);
			zbPlayerTipsWallItem2.Refresh(item);
		}
		m_vwWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_WallItem.Count <= 0));
	}

	public void RefreshGun(RoleTipsDetail roleTipsDetail)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerGunDetail> list = ((IEnumerable<PlayerGunDetail>)roleTipsDetail.GunList).ToList();
		list.Sort(delegate(PlayerGunDetail p1, PlayerGunDetail p2)
		{
			zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(p1.GunId);
			int num = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(p2.GunId).quality.CompareTo(recordByKey.quality);
			if (num != 0)
			{
				return num;
			}
			num = p2.Star.CompareTo(p1.Star);
			return (num != 0) ? num : p1.GunId.CompareTo(p2.GunId);
		});
		m_GunItem.Clear();
		((VisualElement)m_scViewGun).Clear();
		foreach (PlayerGunDetail item in list)
		{
			zbPlayerTipsGunItem zbPlayerTipsGunItem2 = new zbPlayerTipsGunItem(this);
			m_GunItem.Add(zbPlayerTipsGunItem2);
			((VisualElement)m_scViewGun).Add((VisualElement)(object)zbPlayerTipsGunItem2);
			zbPlayerTipsGunItem2.Refresh(item);
		}
		m_vwGun.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_GunItem.Count <= 0));
	}

	public void RefreshMecha(RoleTipsDetail roleTipsDetail)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		List<MechaDetail> list = ((IEnumerable<MechaDetail>)roleTipsDetail.MechaList).ToList();
		list.Sort(delegate(MechaDetail p1, MechaDetail p2)
		{
			zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(p1.MechaId);
			int num = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(p2.MechaId).quality.CompareTo(recordByKey.quality);
			if (num != 0)
			{
				return num;
			}
			num = p2.Star.CompareTo(p1.Star);
			return (num != 0) ? num : p1.MechaId.CompareTo(p2.MechaId);
		});
		m_MechaItem.Clear();
		((VisualElement)m_scViewMecha).Clear();
		foreach (MechaDetail item in list)
		{
			zbPlayerTipsMechaItem zbPlayerTipsMechaItem2 = new zbPlayerTipsMechaItem(this);
			m_MechaItem.Add(zbPlayerTipsMechaItem2);
			((VisualElement)m_scViewMecha).Add((VisualElement)(object)zbPlayerTipsMechaItem2);
			zbPlayerTipsMechaItem2.Refresh(item);
		}
		m_vwMecha.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_MechaItem.Count <= 0));
	}

	public void RefreshFoothold(RoleTipsDetail roleTipsDetail)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		List<FootholdDetail> list = ((IEnumerable<FootholdDetail>)roleTipsDetail.FootholdList).ToList();
		list.Sort(delegate(FootholdDetail p1, FootholdDetail p2)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(p1.FootholdId);
			int num = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(p2.FootholdId).quality.CompareTo(recordByKey.quality);
			if (num != 0)
			{
				return num;
			}
			num = p2.Star.CompareTo(p1.Star);
			return (num != 0) ? num : p1.FootholdId.CompareTo(p2.FootholdId);
		});
		m_FootholdItem.Clear();
		((VisualElement)m_scViewFoothold).Clear();
		foreach (FootholdDetail item in list)
		{
			zbPlayerTipsFootholdItem zbPlayerTipsFootholdItem2 = new zbPlayerTipsFootholdItem(this);
			m_FootholdItem.Add(zbPlayerTipsFootholdItem2);
			((VisualElement)m_scViewFoothold).Add((VisualElement)(object)zbPlayerTipsFootholdItem2);
			zbPlayerTipsFootholdItem2.Refresh(item);
		}
		m_vwFoothold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_FootholdItem.Count <= 0));
	}

	private void OnClose()
	{
		Hide();
	}
}
