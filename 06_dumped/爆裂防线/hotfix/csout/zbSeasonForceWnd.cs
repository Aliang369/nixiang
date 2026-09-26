using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonForceWnd : zbBaseWnd
{
	private VisualElement m_spriteForce;

	private Label m_lbMember;

	private Label m_lbLevel;

	private VisualElement m_spriteProgress;

	private Label m_lbProgress;

	private Label m_lbNotice;

	private Button m_btnTrans;

	private Button m_btnNotice;

	private Button m_btnCity;

	private Button m_btnRank;

	private Button m_btnTech;

	private Button m_btnMember;

	private Button m_btnShop;

	private Button m_btnManage;

	private Button m_btnClose;

	private SeasonForceInfo m_info;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spriteForce = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_force", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_lbNotice = UQueryExtensions.Q<Label>(m_RootElement, "lb_notice", (string)null);
		m_btnTrans = UQueryExtensions.Q<Button>(m_RootElement, "btn_trans", (string)null);
		m_btnNotice = UQueryExtensions.Q<Button>(m_RootElement, "btn_notice", (string)null);
		m_btnCity = UQueryExtensions.Q<Button>(m_RootElement, "btn_city", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCity, (Action<ClickEvent>)OnCity, "", (TrickleDown)0);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action<ClickEvent>)OnRank, "", (TrickleDown)0);
		m_btnTech = UQueryExtensions.Q<Button>(m_RootElement, "btn_tech", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTech, (Action<ClickEvent>)OnTech, "", (TrickleDown)0);
		m_btnMember = UQueryExtensions.Q<Button>(m_RootElement, "btn_member", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMember, (Action<ClickEvent>)OnMember, "", (TrickleDown)0);
		m_btnShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_shop", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShop, (Action<ClickEvent>)OnShop, "", (TrickleDown)0);
		m_btnManage = UQueryExtensions.Q<Button>(m_RootElement, "btn_manage", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnManage, (Action<ClickEvent>)OnManage, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNotice, (Action)OnNotice, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTrans, (Action)OnTrans, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(SeasonForceInfo info)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		m_info = info;
		if (m_info != null)
		{
			((VisualElement)m_btnNotice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_info.Leader?.RoleId != zbSelf.Instance.GetRoleId()) ? 1 : 0));
			m_spriteForce.style.backgroundImage = zbSelf.Instance.seasonHandler.GetForceBigFlagByForceId(m_info.ForceId);
			((TextElement)m_lbMember).text = m_info.MemberCount.ToString();
			((TextElement)m_lbLevel).text = "Lv." + m_info.Level;
			zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(m_info.Level);
			zbLegionLevelCsvRecord recordByKey = zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(m_info.Level + 1);
			bool num = recordByKey != null;
			int exp = m_info.Exp;
			if (num)
			{
				int exp2 = recordByKey.exp;
				((TextElement)m_lbProgress).text = $"{exp} / {exp2}";
				m_spriteProgress.style.width = new StyleLength(new Length((float)exp / (float)exp2 * 100f, (LengthUnit)1));
			}
			else
			{
				((TextElement)m_lbProgress).text = $"{exp} / Max";
				m_spriteProgress.style.width = new StyleLength(new Length(100f, (LengthUnit)1));
			}
			((TextElement)m_lbNotice).text = m_info.Notice;
		}
	}

	private void OnTrans()
	{
	}

	private void OnNotice()
	{
		zbUIMgr.Instance.ShowLegionNotice();
	}

	private void OnCity(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSeasonForceCity();
	}

	private void OnMember(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Season_Force_Members_Req();
	}

	private void OnManage(ClickEvent evt)
	{
	}

	private void OnRank(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSeasonRank();
	}

	private void OnTech(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Season_Tech_Info_Req();
	}

	private void OnShop(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Season_Shop_List_Req();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
