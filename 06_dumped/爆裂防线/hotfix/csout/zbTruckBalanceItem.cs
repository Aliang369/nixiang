using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckBalanceItem
{
	public VisualElement m_rootElement;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbTime;

	private Label m_lbDesc;

	private Label m_lbResult;

	private Button m_btnHead;

	private Button m_btnTongji;

	private VisualElement m_bgTongji;

	private zbItem m_item;

	private TruckRobRecordDetail m_detail;

	private zbTruckBalanceWnd wndParent;

	public void OnEnable(zbTruckBalanceWnd _wndParent, VisualElement root)
	{
		wndParent = _wndParent;
		m_rootElement = root;
		m_lbTime = UQueryExtensions.Q<Label>(m_rootElement, "lb_time", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_rootElement, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>(m_rootElement, "lb_power", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_rootElement, "lb_desc", (string)null);
		m_lbResult = UQueryExtensions.Q<Label>(m_rootElement, "lb_result", (string)null);
		m_bgTongji = UQueryExtensions.Q<VisualElement>(m_rootElement, "bg_tongji", (string)null);
		m_btnTongji = UQueryExtensions.Q<Button>(m_rootElement, "btn_tongji", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_rootElement, "btn_head", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTongji, (Action)OnTongji, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnDetailHead, "", (TrickleDown)0);
		m_item = UQueryExtensions.Q<zbItem>(m_rootElement, "rob_award", (string)null);
		m_item?.OnEnable(wndParent);
	}

	public void Refresh(TruckRobRecordDetail detail)
	{
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		InitPlayerInfo();
		((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(m_detail.RoleBaseInfo.TeamAttack);
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_detail.RecordTm);
		if (m_detail.Award != null)
		{
			zbItemData data = new zbItemData(m_detail.Award.Type, m_detail.Award.Tid, m_detail.Award.Count);
			m_item.Refresh(data);
			((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbServerData.sGameData.TruckSelfInfo.WantedList.Contains((int)m_detail.RoleBaseInfo.RoleId))
			{
				((VisualElement)m_btnTongji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_bgTongji.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)m_btnTongji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_bgTongji.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.11474");
			((TextElement)m_lbResult).text = zbUtil.Language("lang.11152");
			((VisualElement)m_lbResult).style.color = StyleColor.op_Implicit(new Color(0.6901961f, 0f, 0f));
		}
		else
		{
			((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnTongji).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bgTongji.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.11150");
			((TextElement)m_lbResult).text = zbUtil.Language("lang.11151");
			((VisualElement)m_lbResult).style.color = StyleColor.op_Implicit(new Color(0f, 0.4627451f, 0.03137255f));
		}
	}

	private void InitPlayerInfo()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = zbUtil.SetServerString(m_detail.RoleBaseInfo.ServerId) + "-" + m_detail.RoleBaseInfo.RoleName;
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_rootElement, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_rootElement, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
	}

	private void OnTongji()
	{
		if (!zbUtil.IsNpc(m_detail.RoleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Truck_AddWanted_Req((int)m_detail.RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
	}

	private void OnDetailHead()
	{
		if (!zbUtil.IsNpc(m_detail.RoleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
	}
}
